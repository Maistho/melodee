using Melodee.Common.Enums;

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Summary DTO for request list views.
/// </summary>
public record RequestSummary(
    Guid ApiKey,
    string Category,
    string Status,
    string Description,
    string? ArtistName,
    string? AlbumTitle,
    string? SongTitle,
    int? ReleaseYear,
    string CreatedAt,
    string UpdatedAt,
    string LastActivityAt,
    string LastActivityType,
    int CommentCount,
    UserSummary CreatedByUser);

/// <summary>
/// Detail DTO for request single view.
/// </summary>
public record RequestDetail(
    Guid ApiKey,
    string Category,
    string Status,
    string Description,
    string? ArtistName,
    Guid? TargetArtistApiKey,
    string? AlbumTitle,
    Guid? TargetAlbumApiKey,
    string? SongTitle,
    Guid? TargetSongApiKey,
    int? ReleaseYear,
    string? ExternalUrl,
    string? Notes,
    string CreatedAt,
    string UpdatedAt,
    string LastActivityAt,
    string LastActivityType,
    int CommentCount,
    UserSummary CreatedByUser,
    UserSummary? LastActivityUser);

/// <summary>
/// Minimal user info for embedding in responses.
/// </summary>
public record UserSummary(
    Guid ApiKey,
    string UserName);

/// <summary>
/// Request to create a new request.
/// </summary>
public record CreateRequestRequest(
    RequestCategory Category,
    string Description,
    string? ArtistName = null,
    Guid? TargetArtistApiKey = null,
    string? AlbumTitle = null,
    Guid? TargetAlbumApiKey = null,
    string? SongTitle = null,
    Guid? TargetSongApiKey = null,
    int? ReleaseYear = null,
    string? ExternalUrl = null,
    string? Notes = null);

/// <summary>
/// Request to update an existing request.
/// </summary>
public record UpdateRequestRequest(
    string? Description = null,
    string? ArtistName = null,
    Guid? TargetArtistApiKey = null,
    string? AlbumTitle = null,
    Guid? TargetAlbumApiKey = null,
    string? SongTitle = null,
    Guid? TargetSongApiKey = null,
    int? ReleaseYear = null,
    string? ExternalUrl = null,
    string? Notes = null);

/// <summary>
/// Paged response for requests.
/// </summary>
public record RequestPagedResponse(PaginationMetadata Meta, RequestSummary[] Data);

/// <summary>
/// Comment DTO for request comments.
/// </summary>
public record RequestCommentDto(
    Guid ApiKey,
    Guid? ParentCommentApiKey,
    string Body,
    bool IsSystem,
    string CreatedAt,
    UserSummary? CreatedByUser);

/// <summary>
/// Request to create a new comment.
/// </summary>
public record CreateCommentRequest(
    string Body,
    Guid? ParentCommentApiKey = null);

/// <summary>
/// Paged response for comments.
/// </summary>
public record CommentPagedResponse(PaginationMetadata Meta, RequestCommentDto[] Data);

/// <summary>
/// Activity check response.
/// </summary>
public record ActivityCheckResponse(bool HasUnread);

/// <summary>
/// Unread request summary for activity feed.
/// </summary>
public record UnreadRequestSummary(
    Guid ApiKey,
    string Category,
    string Status,
    string Description,
    string LastActivityAt,
    string LastActivityType,
    UserSummary? LastActivityUser);
