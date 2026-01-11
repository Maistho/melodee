namespace Melodee.Common.Constants;

public static class SettingDefaults
{
    public const string DefaultSiteName = "Melodee";
    public const int ValidationMinimumAlbumYear = 1900;
    public const int ValidationMaximumSongNumber = 1000;
    public const int ValidationMinimumSongCount = 1;
    public const int ValidationMinimumAlbumDuration = 1;
    public const int PodcastHttpMaxRedirects = 10;

    /// <summary>
    /// Default gain/volume level for jukebox (0.0 to 1.0 scale).
    /// </summary>
    public const double JukeboxDefaultGain = 0.8;
}
