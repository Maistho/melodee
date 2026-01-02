using System.Text.Json.Serialization;

namespace Melodee.Blazor.Controllers.Jellyfin.Models;

public record JellyfinUserView
{
    [JsonPropertyName("Name")]
    public required string Name { get; init; }

    [JsonPropertyName("ServerId")]
    public required string ServerId { get; init; }

    [JsonPropertyName("Id")]
    public required string Id { get; init; }

    [JsonPropertyName("Etag")]
    public string? Etag { get; init; }

    [JsonPropertyName("DateCreated")]
    public string? DateCreated { get; init; }

    [JsonPropertyName("CanDelete")]
    public bool CanDelete { get; init; }

    [JsonPropertyName("CanDownload")]
    public bool CanDownload { get; init; }

    [JsonPropertyName("SortName")]
    public string? SortName { get; init; }

    [JsonPropertyName("ExternalUrls")]
    public object[]? ExternalUrls { get; init; }

    [JsonPropertyName("Path")]
    public string? Path { get; init; }

    [JsonPropertyName("EnableMediaSourceDisplay")]
    public bool EnableMediaSourceDisplay { get; init; } = true;

    [JsonPropertyName("ChannelId")]
    public string? ChannelId { get; init; }

    [JsonPropertyName("Taglines")]
    public string[]? Taglines { get; init; }

    [JsonPropertyName("Genres")]
    public string[]? Genres { get; init; }

    [JsonPropertyName("PlayAccess")]
    public string PlayAccess { get; init; } = "Full";

    [JsonPropertyName("RemoteTrailers")]
    public object[]? RemoteTrailers { get; init; }

    [JsonPropertyName("ProviderIds")]
    public Dictionary<string, string>? ProviderIds { get; init; }

    [JsonPropertyName("IsFolder")]
    public bool IsFolder { get; init; } = true;

    [JsonPropertyName("ParentId")]
    public string? ParentId { get; init; }

    [JsonPropertyName("Type")]
    public string Type { get; init; } = "UserView";

    [JsonPropertyName("Studios")]
    public object[]? Studios { get; init; }

    [JsonPropertyName("GenreItems")]
    public object[]? GenreItems { get; init; }

    [JsonPropertyName("LocalTrailerCount")]
    public int LocalTrailerCount { get; init; }

    [JsonPropertyName("UserData")]
    public JellyfinUserItemData? UserData { get; init; }

    [JsonPropertyName("ChildCount")]
    public int ChildCount { get; init; }

    [JsonPropertyName("SpecialFeatureCount")]
    public int SpecialFeatureCount { get; init; }

    [JsonPropertyName("DisplayPreferencesId")]
    public string? DisplayPreferencesId { get; init; }

    [JsonPropertyName("Tags")]
    public string[]? Tags { get; init; }

    [JsonPropertyName("PrimaryImageAspectRatio")]
    public double? PrimaryImageAspectRatio { get; init; }

    [JsonPropertyName("CollectionType")]
    public string? CollectionType { get; init; }

    [JsonPropertyName("ImageTags")]
    public Dictionary<string, string>? ImageTags { get; init; }

    [JsonPropertyName("BackdropImageTags")]
    public string[]? BackdropImageTags { get; init; }

    [JsonPropertyName("ScreenshotImageTags")]
    public string[]? ScreenshotImageTags { get; init; }

    [JsonPropertyName("ImageBlurHashes")]
    public Dictionary<string, Dictionary<string, string>>? ImageBlurHashes { get; init; }

    [JsonPropertyName("LocationType")]
    public string LocationType { get; init; } = "Virtual";

    [JsonPropertyName("LockedFields")]
    public string[]? LockedFields { get; init; }

    [JsonPropertyName("LockData")]
    public bool LockData { get; init; }
}

public record JellyfinUserItemData
{
    [JsonPropertyName("PlaybackPositionTicks")]
    public long PlaybackPositionTicks { get; init; }

    [JsonPropertyName("PlayCount")]
    public int PlayCount { get; init; }

    [JsonPropertyName("IsFavorite")]
    public bool IsFavorite { get; init; }

    [JsonPropertyName("Played")]
    public bool Played { get; init; }

    [JsonPropertyName("Key")]
    public string? Key { get; init; }

    [JsonPropertyName("LastPlayedDate")]
    public string? LastPlayedDate { get; init; }
}

public record JellyfinUserViewsResult
{
    [JsonPropertyName("Items")]
    public required JellyfinUserView[] Items { get; init; }

    [JsonPropertyName("TotalRecordCount")]
    public int TotalRecordCount { get; init; }

    [JsonPropertyName("StartIndex")]
    public int StartIndex { get; init; }
}

public record JellyfinBaseItem
{
    [JsonPropertyName("Name")]
    public required string Name { get; init; }

    [JsonPropertyName("ServerId")]
    public required string ServerId { get; init; }

    [JsonPropertyName("Id")]
    public required string Id { get; init; }

    [JsonPropertyName("Etag")]
    public string? Etag { get; init; }

    [JsonPropertyName("DateCreated")]
    public string? DateCreated { get; init; }

    [JsonPropertyName("CanDelete")]
    public bool CanDelete { get; init; }

    [JsonPropertyName("CanDownload")]
    public bool CanDownload { get; init; } = true;

    [JsonPropertyName("SortName")]
    public string? SortName { get; init; }

    [JsonPropertyName("ExternalUrls")]
    public object[]? ExternalUrls { get; init; }

    [JsonPropertyName("Path")]
    public string? Path { get; init; }

    [JsonPropertyName("EnableMediaSourceDisplay")]
    public bool EnableMediaSourceDisplay { get; init; } = true;

    [JsonPropertyName("ChannelId")]
    public string? ChannelId { get; init; }

    [JsonPropertyName("Overview")]
    public string? Overview { get; init; }

    [JsonPropertyName("Taglines")]
    public string[]? Taglines { get; init; }

    [JsonPropertyName("Genres")]
    public string[]? Genres { get; init; }

    [JsonPropertyName("CommunityRating")]
    public double? CommunityRating { get; init; }

    [JsonPropertyName("RunTimeTicks")]
    public long? RunTimeTicks { get; init; }

    [JsonPropertyName("PlayAccess")]
    public string PlayAccess { get; init; } = "Full";

    [JsonPropertyName("ProductionYear")]
    public int? ProductionYear { get; init; }

    [JsonPropertyName("IndexNumber")]
    public int? IndexNumber { get; init; }

    [JsonPropertyName("ParentIndexNumber")]
    public int? ParentIndexNumber { get; init; }

    [JsonPropertyName("RemoteTrailers")]
    public object[]? RemoteTrailers { get; init; }

    [JsonPropertyName("ProviderIds")]
    public Dictionary<string, string>? ProviderIds { get; init; }

    [JsonPropertyName("IsFolder")]
    public bool IsFolder { get; init; }

    [JsonPropertyName("ParentId")]
    public string? ParentId { get; init; }

    [JsonPropertyName("Type")]
    public required string Type { get; init; }

    [JsonPropertyName("Studios")]
    public object[]? Studios { get; init; }

    [JsonPropertyName("GenreItems")]
    public object[]? GenreItems { get; init; }

    [JsonPropertyName("LocalTrailerCount")]
    public int LocalTrailerCount { get; init; }

    [JsonPropertyName("UserData")]
    public JellyfinUserItemData? UserData { get; init; }

    [JsonPropertyName("ChildCount")]
    public int? ChildCount { get; init; }

    [JsonPropertyName("SpecialFeatureCount")]
    public int SpecialFeatureCount { get; init; }

    [JsonPropertyName("DisplayPreferencesId")]
    public string? DisplayPreferencesId { get; init; }

    [JsonPropertyName("Tags")]
    public string[]? Tags { get; init; }

    [JsonPropertyName("PrimaryImageAspectRatio")]
    public double? PrimaryImageAspectRatio { get; init; }

    [JsonPropertyName("Artists")]
    public string[]? Artists { get; init; }

    [JsonPropertyName("ArtistItems")]
    public JellyfinNameGuidPair[]? ArtistItems { get; init; }

    [JsonPropertyName("Album")]
    public string? Album { get; init; }

    [JsonPropertyName("AlbumId")]
    public string? AlbumId { get; init; }

    [JsonPropertyName("AlbumPrimaryImageTag")]
    public string? AlbumPrimaryImageTag { get; init; }

    [JsonPropertyName("AlbumArtist")]
    public string? AlbumArtist { get; init; }

    [JsonPropertyName("AlbumArtists")]
    public JellyfinNameGuidPair[]? AlbumArtists { get; init; }

    [JsonPropertyName("ImageTags")]
    public Dictionary<string, string>? ImageTags { get; init; }

    [JsonPropertyName("BackdropImageTags")]
    public string[]? BackdropImageTags { get; init; }

    [JsonPropertyName("ScreenshotImageTags")]
    public string[]? ScreenshotImageTags { get; init; }

    [JsonPropertyName("ImageBlurHashes")]
    public Dictionary<string, Dictionary<string, string>>? ImageBlurHashes { get; init; }

    [JsonPropertyName("LocationType")]
    public string LocationType { get; init; } = "FileSystem";

    [JsonPropertyName("LockedFields")]
    public string[]? LockedFields { get; init; }

    [JsonPropertyName("LockData")]
    public bool LockData { get; init; }

    [JsonPropertyName("MediaSources")]
    public JellyfinMediaSource[]? MediaSources { get; init; }

    [JsonPropertyName("Container")]
    public string? Container { get; init; }

    [JsonPropertyName("MediaType")]
    public string? MediaType { get; init; }

    [JsonPropertyName("HasLyrics")]
    public bool HasLyrics { get; init; }
}

public record JellyfinNameGuidPair
{
    [JsonPropertyName("Name")]
    public required string Name { get; init; }

    [JsonPropertyName("Id")]
    public required string Id { get; init; }
}

public record JellyfinMediaSource
{
    [JsonPropertyName("Protocol")]
    public string Protocol { get; init; } = "File";

    [JsonPropertyName("Id")]
    public required string Id { get; init; }

    [JsonPropertyName("Path")]
    public string? Path { get; init; }

    [JsonPropertyName("Type")]
    public string Type { get; init; } = "Default";

    [JsonPropertyName("Container")]
    public string? Container { get; init; }

    [JsonPropertyName("Size")]
    public long? Size { get; init; }

    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("IsRemote")]
    public bool IsRemote { get; init; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; init; }

    [JsonPropertyName("RunTimeTicks")]
    public long? RunTimeTicks { get; init; }

    [JsonPropertyName("ReadAtNativeFramerate")]
    public bool ReadAtNativeFramerate { get; init; }

    [JsonPropertyName("IgnoreDts")]
    public bool IgnoreDts { get; init; }

    [JsonPropertyName("IgnoreIndex")]
    public bool IgnoreIndex { get; init; }

    [JsonPropertyName("GenPtsInput")]
    public bool GenPtsInput { get; init; }

    [JsonPropertyName("SupportsTranscoding")]
    public bool SupportsTranscoding { get; init; } = true;

    [JsonPropertyName("SupportsDirectStream")]
    public bool SupportsDirectStream { get; init; } = true;

    [JsonPropertyName("SupportsDirectPlay")]
    public bool SupportsDirectPlay { get; init; } = true;

    [JsonPropertyName("IsInfiniteStream")]
    public bool IsInfiniteStream { get; init; }

    [JsonPropertyName("RequiresOpening")]
    public bool RequiresOpening { get; init; }

    [JsonPropertyName("RequiresClosing")]
    public bool RequiresClosing { get; init; }

    [JsonPropertyName("RequiresLooping")]
    public bool RequiresLooping { get; init; }

    [JsonPropertyName("SupportsProbing")]
    public bool SupportsProbing { get; init; } = true;

    [JsonPropertyName("MediaStreams")]
    public JellyfinMediaStream[]? MediaStreams { get; init; }

    [JsonPropertyName("Bitrate")]
    public int? Bitrate { get; init; }

    [JsonPropertyName("DefaultAudioStreamIndex")]
    public int? DefaultAudioStreamIndex { get; init; }
}

public record JellyfinMediaStream
{
    [JsonPropertyName("Codec")]
    public string? Codec { get; init; }

    [JsonPropertyName("TimeBase")]
    public string? TimeBase { get; init; }

    [JsonPropertyName("CodecTimeBase")]
    public string? CodecTimeBase { get; init; }

    [JsonPropertyName("Title")]
    public string? Title { get; init; }

    [JsonPropertyName("DisplayTitle")]
    public string? DisplayTitle { get; init; }

    [JsonPropertyName("DisplayLanguage")]
    public string? DisplayLanguage { get; init; }

    [JsonPropertyName("IsInterlaced")]
    public bool IsInterlaced { get; init; }

    [JsonPropertyName("IsAVC")]
    public bool? IsAVC { get; init; }

    [JsonPropertyName("ChannelLayout")]
    public string? ChannelLayout { get; init; }

    [JsonPropertyName("BitRate")]
    public int? BitRate { get; init; }

    [JsonPropertyName("BitDepth")]
    public int? BitDepth { get; init; }

    [JsonPropertyName("RefFrames")]
    public int? RefFrames { get; init; }

    [JsonPropertyName("IsDefault")]
    public bool IsDefault { get; init; }

    [JsonPropertyName("IsForced")]
    public bool IsForced { get; init; }

    [JsonPropertyName("Type")]
    public string Type { get; init; } = "Audio";

    [JsonPropertyName("Index")]
    public int Index { get; init; }

    [JsonPropertyName("IsExternal")]
    public bool IsExternal { get; init; }

    [JsonPropertyName("IsTextSubtitleStream")]
    public bool IsTextSubtitleStream { get; init; }

    [JsonPropertyName("SupportsExternalStream")]
    public bool SupportsExternalStream { get; init; }

    [JsonPropertyName("Level")]
    public int Level { get; init; }

    [JsonPropertyName("Channels")]
    public int? Channels { get; init; }

    [JsonPropertyName("SampleRate")]
    public int? SampleRate { get; init; }

    [JsonPropertyName("IsAnamorphic")]
    public bool? IsAnamorphic { get; init; }
}

public record JellyfinItemsResult
{
    [JsonPropertyName("Items")]
    public required JellyfinBaseItem[] Items { get; init; }

    [JsonPropertyName("TotalRecordCount")]
    public int TotalRecordCount { get; init; }

    [JsonPropertyName("StartIndex")]
    public int StartIndex { get; init; }
}

public record JellyfinArtistsResult
{
    [JsonPropertyName("Items")]
    public required JellyfinBaseItem[] Items { get; init; }

    [JsonPropertyName("TotalRecordCount")]
    public int TotalRecordCount { get; init; }

    [JsonPropertyName("StartIndex")]
    public int StartIndex { get; init; }
}

public record JellyfinPlaybackInfoResult
{
    [JsonPropertyName("MediaSources")]
    public required JellyfinMediaSource[] MediaSources { get; init; }

    [JsonPropertyName("PlaySessionId")]
    public string? PlaySessionId { get; init; }
}

public record JellyfinCreatePlaylistRequest
{
    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("Ids")]
    public string[]? Ids { get; init; }

    [JsonPropertyName("UserId")]
    public string? UserId { get; init; }

    [JsonPropertyName("MediaType")]
    public string? MediaType { get; init; }

    [JsonPropertyName("Users")]
    public JellyfinPlaylistUserPermissions[]? Users { get; init; }

    [JsonPropertyName("IsPublic")]
    public bool? IsPublic { get; init; }
}

public record JellyfinPlaylistUserPermissions
{
    [JsonPropertyName("UserId")]
    public string? UserId { get; init; }

    [JsonPropertyName("CanEdit")]
    public bool CanEdit { get; init; }
}

public record JellyfinCreatePlaylistResponse
{
    [JsonPropertyName("Id")]
    public required string Id { get; init; }
}

public record JellyfinUpdatePlaylistRequest
{
    [JsonPropertyName("Name")]
    public string? Name { get; init; }

    [JsonPropertyName("IsPublic")]
    public bool? IsPublic { get; init; }

    [JsonPropertyName("MediaType")]
    public string? MediaType { get; init; }

    [JsonPropertyName("Genres")]
    public JellyfinNameGuidPair[]? Genres { get; init; }

    [JsonPropertyName("Tags")]
    public JellyfinNameGuidPair[]? Tags { get; init; }

    [JsonPropertyName("UserId")]
    public string? UserId { get; init; }

    [JsonPropertyName("PremiereDate")]
    public string? PremiereDate { get; init; }

    [JsonPropertyName("ProviderIds")]
    public Dictionary<string, string>? ProviderIds { get; init; }
}
