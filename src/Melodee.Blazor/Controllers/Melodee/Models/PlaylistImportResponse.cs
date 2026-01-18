namespace Melodee.Blazor.Controllers.Melodee.Models;

public sealed class PlaylistImportResponse
{
    public required Guid PlaylistId { get; init; }
    public required string PlaylistName { get; init; }
    public required int TotalEntries { get; init; }
    public required int MatchedEntries { get; init; }
    public required int MissingEntries { get; init; }
}
