using Melodee.Blazor.Controllers.Melodee.Models;
using MelodeeDataModels = Melodee.Common.Data.Models;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class ShareExtensions
{
    public static Share ToShareModel(
        this MelodeeDataModels.Share share,
        string baseUrl,
        User owner,
        string shareType,
        Guid resourceApiKey,
        string resourceName,
        string resourceThumbnailUrl,
        string resourceImageUrl)
    {
        return new Share(
            share.ApiKey,
            $"{baseUrl}/share/{share.ShareUniqueId}",
            shareType,
            resourceApiKey,
            resourceName,
            resourceThumbnailUrl,
            resourceImageUrl,
            share.Description,
            share.IsDownloadable,
            share.VisitCount,
            owner,
            share.CreatedAt.ToString(),
            share.ExpiresAt?.ToString(),
            share.LastVisitedAt?.ToString()
        );
    }
}
