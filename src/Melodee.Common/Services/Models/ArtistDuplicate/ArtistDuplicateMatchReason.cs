namespace Melodee.Common.Services.Models.ArtistDuplicate;

/// <summary>
/// Reasons why two artists are considered potential duplicates.
/// </summary>
public static class ArtistDuplicateMatchReason
{
    public const string SharedSpotifyId = "SharedSpotifyId";
    public const string SharedMusicBrainzId = "SharedMusicBrainzId";
    public const string SharedDiscogsId = "SharedDiscogsId";
    public const string SharedItunesId = "SharedItunesId";
    public const string SharedDeezerId = "SharedDeezerId";
    public const string SharedLastFmId = "SharedLastFmId";
    public const string SharedAmgId = "SharedAmgId";
    public const string SharedWikiDataId = "SharedWikiDataId";
    public const string MultipleSharedExternalIds = "MultipleSharedExternalIds";
    public const string ExactNormalizedNameMatch = "ExactNormalizedNameMatch";
    public const string NameFirstLastReversal = "NameFirstLastReversal";
    public const string HighNameSimilarity = "HighNameSimilarity";
    public const string HighTokenSimilarity = "HighTokenSimilarity";
    public const string SharedAlbums = "SharedAlbums";
    public const string HighAlbumOverlap = "HighAlbumOverlap";
}
