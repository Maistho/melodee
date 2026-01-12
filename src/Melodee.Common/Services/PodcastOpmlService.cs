using System.Text;
using System.Xml;
using System.Xml.Linq;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Common.Services;

/// <summary>
/// Service for OPML (Outline Processor Markup Language) import and export of podcast subscriptions.
/// OPML is a standard format for podcast subscription backup and migration between applications.
/// </summary>
public sealed class PodcastOpmlService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    PodcastService podcastService) : ServiceBase(logger, cacheManager, contextFactory)
{
    /// <summary>
    /// Exports all podcast subscriptions for a user to OPML format.
    /// </summary>
    public async Task<OperationResult<string>> ExportAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            var channels = await context.PodcastChannels
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .OrderBy(c => c.Title)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("opml",
                    new XAttribute("version", "2.0"),
                    new XElement("head",
                        new XElement("title", "Melodee Podcast Subscriptions"),
                        new XElement("dateCreated", DateTime.UtcNow.ToString("R")),
                        new XElement("docs", "http://opml.org/spec2.opml")),
                    new XElement("body",
                        new XElement("outline",
                            new XAttribute("text", "Podcasts"),
                            new XAttribute("title", "Podcasts"),
                            channels.Select(c => new XElement("outline",
                                new XAttribute("type", "rss"),
                                new XAttribute("text", c.Title ?? "Untitled"),
                                new XAttribute("title", c.Title ?? "Untitled"),
                                new XAttribute("xmlUrl", c.FeedUrl),
                                c.SiteUrl != null ? new XAttribute("htmlUrl", c.SiteUrl) : null,
                                c.Description != null ? new XAttribute("description", c.Description) : null))))));

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using var stringWriter = new StringWriter();
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                doc.Save(xmlWriter);
            }

            Logger.Information("[{ServiceName}] Exported {Count} podcast subscriptions to OPML for user {UserId}",
                nameof(PodcastOpmlService), channels.Count, userId);

            return new OperationResult<string> { Data = stringWriter.ToString() };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error exporting OPML for user {UserId}", nameof(PodcastOpmlService), userId);
            return new OperationResult<string>(OperationResponseType.Error, ex.Message) { Data = string.Empty };
        }
    }

    /// <summary>
    /// Imports podcast subscriptions from OPML format.
    /// Returns the number of channels successfully imported.
    /// </summary>
    public async Task<OperationResult<OpmlImportResult>> ImportAsync(
        int userId,
        string opmlContent,
        CancellationToken cancellationToken = default)
    {
        var result = new OpmlImportResult();

        try
        {
            var doc = XDocument.Parse(opmlContent);

            var outlines = doc.Descendants("outline")
                .Where(o => o.Attribute("type")?.Value?.Equals("rss", StringComparison.OrdinalIgnoreCase) == true
                         || o.Attribute("xmlUrl") != null)
                .ToList();

            if (outlines.Count == 0)
            {
                return new OperationResult<OpmlImportResult>(OperationResponseType.ValidationFailure, "No podcast feeds found in OPML file")
                {
                    Data = result
                };
            }

            await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            // Get existing feed URLs for this user to avoid duplicates
            var existingFeedUrls = await context.PodcastChannels
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Select(c => c.FeedUrl.ToLower())
                .ToHashSetAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var outline in outlines)
            {
                var feedUrl = outline.Attribute("xmlUrl")?.Value;

                if (string.IsNullOrWhiteSpace(feedUrl))
                {
                    result.Skipped++;
                    result.Errors.Add("Outline missing xmlUrl attribute");
                    continue;
                }

                if (existingFeedUrls.Contains(feedUrl.ToLower()))
                {
                    result.Skipped++;
                    result.Duplicates.Add(feedUrl);
                    continue;
                }

                try
                {
                    var createResult = await podcastService.CreateChannelAsync(userId, feedUrl, cancellationToken)
                        .ConfigureAwait(false);

                    if (createResult.IsSuccess)
                    {
                        result.Imported++;
                        result.ImportedFeeds.Add(feedUrl);
                        existingFeedUrls.Add(feedUrl.ToLower());
                    }
                    else
                    {
                        result.Failed++;
                        result.Errors.Add($"{feedUrl}: {createResult.Messages?.FirstOrDefault() ?? "Unknown error"}");
                    }
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"{feedUrl}: {ex.Message}");
                }
            }

            Logger.Information(
                "[{ServiceName}] OPML import for user {UserId}: {Imported} imported, {Skipped} skipped, {Failed} failed",
                nameof(PodcastOpmlService), userId, result.Imported, result.Skipped, result.Failed);

            return new OperationResult<OpmlImportResult> { Data = result };
        }
        catch (XmlException ex)
        {
            Logger.Error(ex, "[{ServiceName}] Invalid OPML XML for user {UserId}", nameof(PodcastOpmlService), userId);
            return new OperationResult<OpmlImportResult>(OperationResponseType.ValidationFailure, $"Invalid OPML format: {ex.Message}") { Data = result };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error importing OPML for user {UserId}", nameof(PodcastOpmlService), userId);
            return new OperationResult<OpmlImportResult>(OperationResponseType.Error, ex.Message) { Data = result };
        }
    }
}

/// <summary>
/// Result of an OPML import operation.
/// </summary>
public sealed class OpmlImportResult
{
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public List<string> ImportedFeeds { get; set; } = [];
    public List<string> Duplicates { get; set; } = [];
    public List<string> Errors { get; set; } = [];

    public int TotalProcessed => Imported + Skipped + Failed;
}
