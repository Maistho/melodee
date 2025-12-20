namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Genre(
    string Id,
    string Name,
    int SongCount,
    int AlbumCount);
