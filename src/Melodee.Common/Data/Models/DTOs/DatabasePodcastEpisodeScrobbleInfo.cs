namespace Melodee.Common.Data.Models.DTOs;

public record DatabasePodcastEpisodeScrobbleInfo(
    int EpisodeId,
    int ChannelId,
    string EpisodeTitle,
    string ChannelTitle,
    string? EpisodeDescription,
    double? Duration,
    string EnclosureUrl,
    string? MimeType,
    Guid? EpisodeKey
);
