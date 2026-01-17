namespace Melodee.Common.Models;

public sealed record PlaylistImportResult
{
    public int PlaylistId { get; init; }
    public Guid PlaylistApiKey { get; init; }
    public string PlaylistName { get; init; } = string.Empty;
    public int TotalEntries { get; init; }
    public int MatchedEntries { get; init; }
    public int MissingEntries { get; init; }
}
