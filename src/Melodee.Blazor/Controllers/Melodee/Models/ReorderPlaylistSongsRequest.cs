namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Request model for reordering songs in a playlist.
/// The SongIds array represents the new order of songs - position in array = new order position.
/// </summary>
public record ReorderPlaylistSongsRequest(Guid[] SongIds);
