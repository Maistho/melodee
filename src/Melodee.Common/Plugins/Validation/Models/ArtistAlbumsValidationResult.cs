using Melodee.Common.Enums;
using Melodee.Common.Models.Validation;

namespace Melodee.Common.Plugins.Validation.Models;

/// <summary>
/// Result of validating all albums for an artist.
/// </summary>
public sealed record ArtistAlbumsValidationResult
{
    public required Guid ArtistApiKey { get; init; }

    public required string ArtistName { get; init; }

    public required int TotalAlbums { get; init; }

    public required int ValidAlbums { get; init; }

    public required int InvalidAlbums { get; init; }

    public required IReadOnlyList<AlbumValidationDetail> AlbumResults { get; init; }

    public bool IsValid => InvalidAlbums == 0;
}

/// <summary>
/// Validation detail for a single album within an artist validation.
/// </summary>
public sealed record AlbumValidationDetail
{
    public required Guid AlbumApiKey { get; init; }

    public required string AlbumName { get; init; }

    public required int? ReleaseYear { get; init; }

    public required bool IsValid { get; init; }

    public required AlbumStatus Status { get; init; }

    public required AlbumNeedsAttentionReasons StatusReasons { get; init; }

    public required IReadOnlyList<ValidationResultMessage> Messages { get; init; }

    public required bool DirectoryExists { get; init; }

    public required bool HasCoverImage { get; init; }

    public required string? DirectoryPath { get; init; }
}
