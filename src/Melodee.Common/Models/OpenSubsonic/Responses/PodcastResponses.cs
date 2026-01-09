using System.Xml.Serialization;
using Melodee.Common.Models.OpenSubsonic.Enums;

namespace Melodee.Common.Models.OpenSubsonic.Responses;

public class PodcastChannelResponse
{
    [XmlAttribute("id")]
    public string Id { get; set; } = string.Empty;

    [XmlAttribute("title")]
    public string Title { get; set; } = string.Empty;

    [XmlAttribute("description")]
    public string? Description { get; set; }

    [XmlAttribute("url")]
    public string Url { get; set; } = string.Empty;

    [XmlAttribute("coverArt")]
    public string? CoverArt { get; set; }

    [XmlAttribute("originalImageUrl")]
    public string? OriginalImageUrl { get; set; }

    [XmlElement("episode")]
    public List<PodcastEpisodeResponse> Episode { get; set; } = [];
}

public class PodcastEpisodeResponse
{
    [XmlAttribute("id")]
    public string Id { get; set; } = string.Empty;

    [XmlAttribute("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    [XmlAttribute("title")]
    public string Title { get; set; } = string.Empty;

    [XmlAttribute("description")]
    public string? Description { get; set; }

    [XmlAttribute("url")]
    public string? Url { get; set; }

    [XmlAttribute("publishDate")]
    public string? PublishDate { get; set; }

    [XmlAttribute("duration")]
    public string? Duration { get; set; }

    [XmlAttribute("status")]
    public string? Status { get; set; }

    [XmlAttribute("fileSize")]
    public long? FileSize { get; set; }

    [XmlAttribute("coverArt")]
    public string? CoverArt { get; set; }

    [XmlElement("stream")]
    public string? StreamId { get; set; }
}

public class PodcastsResponse
{
    [XmlElement("podcasts")]
    public PodcastsContainer Podcasts { get; set; } = new();
}

public class PodcastsContainer
{
    [XmlElement("podcast")]
    public List<PodcastChannelResponse> Channel { get; set; } = [];
}

public class NewestPodcastsResponse
{
    [XmlElement("newestPodcasts")]
    public NewestPodcastsContainer NewestPodcasts { get; set; } = new();
}

public class NewestPodcastsContainer
{
    [XmlElement("episode")]
    public List<PodcastEpisodeResponse> Episode { get; set; } = [];
}
