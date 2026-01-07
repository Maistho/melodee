using Melodee.Blazor.Controllers.Melodee.Models;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class SmartPlaylistExtensions
{
    public static SmartPlaylistModel ToSmartPlaylistModel(
        this Services.SmartPlaylistDto dto,
        string baseUrl,
        User currentUser)
    {
        return new SmartPlaylistModel(
            dto.ApiKey,
            dto.Name,
            dto.MqlQuery,
            dto.EntityType,
            dto.LastResultCount,
            dto.LastEvaluatedAt?.ToString() ?? string.Empty,
            dto.IsPublic,
            dto.NormalizedQuery,
            dto.CreatedAt.ToString(),
            currentUser
        );
    }
}
