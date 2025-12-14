using Melodee.Common.Extensions;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;

namespace Melodee.Common.Plugins.SearchEngine.Brave;

internal static class BraveImageMapper
{
    public static ImageSearchResult? ToImageSearchResult(BraveImageResult source, string fromPlugin)
    {
        if (source == null || string.IsNullOrWhiteSpace(source.Url))
        {
            return null;
        }

        var thumbnailUrl = source.Thumbnail?.Src ?? source.Url;
        var width = source.Properties?.Width ?? 0;
        var height = source.Properties?.Height ?? 0;

        return new ImageSearchResult
        {
            FromPlugin = fromPlugin,
            MediaUrl = source.Url,
            ThumbnailUrl = thumbnailUrl,
            Title = source.Title,
            Width = width,
            Height = height,
            UniqueId = SafeParser.Hash(source.Url),
            Rank = 1
        };
    }

    public static ImageSearchResult[] MapResults(IEnumerable<BraveImageResult> results, int maxResults, string fromPlugin)
    {
        if (results == null)
        {
            return [];
        }

        var mapped = results
            .Select(r => ToImageSearchResult(r, fromPlugin))
            .Where(r => r != null)
            .Cast<ImageSearchResult>()
            .GroupBy(r => r.MediaUrl.ToNormalizedString())
            .Select(g => g.First())
            .Take(maxResults)
            .ToArray();

        return mapped;
    }
}
