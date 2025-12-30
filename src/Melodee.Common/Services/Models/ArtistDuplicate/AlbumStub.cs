namespace Melodee.Common.Services.Models.ArtistDuplicate;

/// <summary>
/// A lightweight stub representing an album for overlap calculation.
/// </summary>
/// <param name="AlbumId">The internal database ID of the album.</param>
/// <param name="Title">The album title.</param>
/// <param name="TitleNormalized">The normalized form of the album title.</param>
/// <param name="Year">The release year (null if unknown).</param>
public sealed record AlbumStub(
    int AlbumId,
    string Title,
    string TitleNormalized,
    int? Year);
