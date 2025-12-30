namespace Melodee.Common.Jobs;

public sealed record ScanStepResult(
    int NewArtistsCount = 0,
    int NewAlbumsCount = 0,
    int NewSongsCount = 0,
    int AlbumsRevalidated = 0,
    int AlbumsNowValid = 0,
    int AlbumsMoved = 0,
    int ArtistsInserted = 0,
    int AlbumsInserted = 0,
    int SongsInserted = 0);
