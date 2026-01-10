using System.Net.Http.Json;
using System.Text.Json;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Common.Services;

/// <summary>
/// Service for discovering and searching podcasts from external directories.
/// Uses iTunes Search API.
/// </summary>
public sealed class PodcastDiscoveryService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    IHttpClientFactory httpClientFactory) : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string ItunesSearchUrl = "https://itunes.apple.com/search";
    private const string CacheRegion = "podcast:discovery";

    /// <summary>
    /// Search for podcasts using the iTunes Search API.
    /// </summary>
    public async Task<OperationResult<PodcastSearchResult>> SearchAsync(
        string query,
        int limit = 25,
        string? country = "US",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new OperationResult<PodcastSearchResult>(OperationResponseType.ValidationFailure, "Search query is required")
            {
                Data = new PodcastSearchResult()
            };
        }

        try
        {
            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
            var podcastEnabled = configuration.GetValue<bool>(SettingRegistry.PodcastEnabled);

            if (!podcastEnabled)
            {
                return new OperationResult<PodcastSearchResult>(OperationResponseType.Error, "Podcast feature is disabled")
                {
                    Data = new PodcastSearchResult()
                };
            }

            var cacheKey = $"search:{query.ToLowerInvariant()}:{limit}:{country}";

            var result = await CacheManager.GetAsync(
                cacheKey,
                async () => await SearchItunesAsync(query, limit, country, cancellationToken).ConfigureAwait(false),
                cancellationToken,
                TimeSpan.FromHours(1),
                CacheRegion).ConfigureAwait(false);

            return new OperationResult<PodcastSearchResult> { Data = result };
        }
        catch (HttpRequestException ex)
        {
            Logger.Error(ex, "[{ServiceName}] HTTP error searching podcasts for '{Query}'", nameof(PodcastDiscoveryService), query);
            return new OperationResult<PodcastSearchResult>(OperationResponseType.Error, $"Error connecting to podcast directory: {ex.Message}")
            {
                Data = new PodcastSearchResult { Query = query }
            };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error searching podcasts for '{Query}'", nameof(PodcastDiscoveryService), query);
            return new OperationResult<PodcastSearchResult>(OperationResponseType.Error, ex.Message)
            {
                Data = new PodcastSearchResult { Query = query }
            };
        }
    }

    private async Task<PodcastSearchResult> SearchItunesAsync(string query, int limit, string? country, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient("PodcastDiscovery");

        var searchUrl = $"{ItunesSearchUrl}?term={Uri.EscapeDataString(query)}&media=podcast&limit={Math.Min(limit, 200)}&country={country}";

        var response = await httpClient.GetAsync(searchUrl, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var itunesResponse = await response.Content.ReadFromJsonAsync<ItunesSearchResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken).ConfigureAwait(false);

        if (itunesResponse == null)
        {
            return new PodcastSearchResult { Query = query };
        }

        var result = new PodcastSearchResult
        {
            Query = query,
            TotalResults = itunesResponse.ResultCount,
            Results = itunesResponse.Results?.Select(r => new PodcastSearchItem
            {
                Title = r.CollectionName ?? r.TrackName ?? "Unknown",
                Artist = r.ArtistName,
                FeedUrl = r.FeedUrl,
                Description = r.Description,
                ImageUrl = r.ArtworkUrl600 ?? r.ArtworkUrl100,
                Genre = r.PrimaryGenreName,
                EpisodeCount = r.TrackCount,
                LastReleaseDate = r.ReleaseDate,
                ItunesUrl = r.CollectionViewUrl,
                ItunesId = r.CollectionId?.ToString()
            }).ToList() ?? []
        };

        Logger.Information("[{ServiceName}] Found {Count} podcasts for query '{Query}'",
            nameof(PodcastDiscoveryService), result.TotalResults, query);

        return result;
    }

    /// <summary>
    /// Get trending/top podcasts from iTunes.
    /// </summary>
    public async Task<OperationResult<PodcastSearchResult>> GetTrendingAsync(
        int limit = 25,
        string? genre = null,
        string? country = "US",
        CancellationToken cancellationToken = default)
    {
        var query = genre ?? "podcast";
        return await SearchAsync(query, limit, country, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lookup a specific podcast by iTunes ID.
    /// </summary>
    public async Task<OperationResult<PodcastSearchItem?>> LookupByItunesIdAsync(
        string itunesId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(itunesId))
        {
            return new OperationResult<PodcastSearchItem?>(OperationResponseType.ValidationFailure, "iTunes ID is required") { Data = null };
        }

        try
        {
            var cacheKey = $"lookup:{itunesId}";

            var result = await CacheManager.GetAsync(
                cacheKey,
                async () => await LookupItunesAsync(itunesId, cancellationToken).ConfigureAwait(false),
                cancellationToken,
                TimeSpan.FromHours(24),
                CacheRegion).ConfigureAwait(false);

            if (result == null)
            {
                return new OperationResult<PodcastSearchItem?>(OperationResponseType.NotFound, "Podcast not found") { Data = null };
            }

            return new OperationResult<PodcastSearchItem?> { Data = result };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{ServiceName}] Error looking up podcast {ItunesId}", nameof(PodcastDiscoveryService), itunesId);
            return new OperationResult<PodcastSearchItem?>(OperationResponseType.Error, ex.Message) { Data = null };
        }
    }

    private async Task<PodcastSearchItem?> LookupItunesAsync(string itunesId, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient("PodcastDiscovery");
        var lookupUrl = $"https://itunes.apple.com/lookup?id={Uri.EscapeDataString(itunesId)}&entity=podcast";

        var response = await httpClient.GetAsync(lookupUrl, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var itunesResponse = await response.Content.ReadFromJsonAsync<ItunesSearchResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken).ConfigureAwait(false);

        var item = itunesResponse?.Results?.FirstOrDefault();
        if (item == null)
        {
            return null;
        }

        return new PodcastSearchItem
        {
            Title = item.CollectionName ?? item.TrackName ?? "Unknown",
            Artist = item.ArtistName,
            FeedUrl = item.FeedUrl,
            Description = item.Description,
            ImageUrl = item.ArtworkUrl600 ?? item.ArtworkUrl100,
            Genre = item.PrimaryGenreName,
            EpisodeCount = item.TrackCount,
            LastReleaseDate = item.ReleaseDate,
            ItunesUrl = item.CollectionViewUrl,
            ItunesId = item.CollectionId?.ToString()
        };
    }
}

#region iTunes API Response Models

internal sealed class ItunesSearchResponse
{
    public int ResultCount { get; set; }
    public List<ItunesResult>? Results { get; set; }
}

internal sealed class ItunesResult
{
    public long? CollectionId { get; set; }
    public string? ArtistName { get; set; }
    public string? CollectionName { get; set; }
    public string? TrackName { get; set; }
    public string? CollectionViewUrl { get; set; }
    public string? FeedUrl { get; set; }
    public string? ArtworkUrl100 { get; set; }
    public string? ArtworkUrl600 { get; set; }
    public string? ReleaseDate { get; set; }
    public string? PrimaryGenreName { get; set; }
    public int? TrackCount { get; set; }
    public string? Description { get; set; }
}

#endregion

#region Public Result Models

/// <summary>
/// Result of a podcast directory search.
/// </summary>
public sealed class PodcastSearchResult
{
    public string Query { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public List<PodcastSearchItem> Results { get; set; } = [];
}

/// <summary>
/// A podcast found in a directory search.
/// </summary>
public sealed class PodcastSearchItem
{
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string? FeedUrl { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Genre { get; set; }
    public int? EpisodeCount { get; set; }
    public string? LastReleaseDate { get; set; }
    public string? ItunesUrl { get; set; }
    public string? ItunesId { get; set; }
}

#endregion
