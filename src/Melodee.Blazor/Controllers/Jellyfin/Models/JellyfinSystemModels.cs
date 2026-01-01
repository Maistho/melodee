using System.Text.Json.Serialization;

namespace Melodee.Blazor.Controllers.Jellyfin.Models;

public record JellyfinPublicSystemInfo
{
    [JsonPropertyName("LocalAddress")]
    public string? LocalAddress { get; init; }

    [JsonPropertyName("ServerName")]
    public required string ServerName { get; init; }

    [JsonPropertyName("Version")]
    public required string Version { get; init; }

    [JsonPropertyName("ProductName")]
    public string ProductName { get; init; } = "Melodee";

    [JsonPropertyName("OperatingSystem")]
    public string? OperatingSystem { get; init; }

    [JsonPropertyName("Id")]
    public required string Id { get; init; }

    [JsonPropertyName("StartupWizardCompleted")]
    public bool? StartupWizardCompleted { get; init; }
}

public record JellyfinSystemInfo : JellyfinPublicSystemInfo
{
    [JsonPropertyName("OperatingSystemDisplayName")]
    public string? OperatingSystemDisplayName { get; init; }

    [JsonPropertyName("HasPendingRestart")]
    public bool HasPendingRestart { get; init; }

    [JsonPropertyName("IsShuttingDown")]
    public bool IsShuttingDown { get; init; }

    [JsonPropertyName("SupportsLibraryMonitor")]
    public bool SupportsLibraryMonitor { get; init; } = true;

    [JsonPropertyName("WebSocketPortNumber")]
    public int WebSocketPortNumber { get; init; }

    [JsonPropertyName("CanSelfRestart")]
    public bool CanSelfRestart { get; init; }

    [JsonPropertyName("CanLaunchWebBrowser")]
    public bool CanLaunchWebBrowser { get; init; }

    [JsonPropertyName("ProgramDataPath")]
    public string? ProgramDataPath { get; init; }

    [JsonPropertyName("ItemsByNamePath")]
    public string? ItemsByNamePath { get; init; }

    [JsonPropertyName("CachePath")]
    public string? CachePath { get; init; }

    [JsonPropertyName("LogPath")]
    public string? LogPath { get; init; }

    [JsonPropertyName("InternalMetadataPath")]
    public string? InternalMetadataPath { get; init; }

    [JsonPropertyName("TranscodingTempPath")]
    public string? TranscodingTempPath { get; init; }

    [JsonPropertyName("HasUpdateAvailable")]
    public bool HasUpdateAvailable { get; init; }
}

public record JellyfinAuthenticationRequest
{
    [JsonPropertyName("Username")]
    public required string Username { get; init; }

    [JsonPropertyName("Pw")]
    public required string Pw { get; init; }
}

public record JellyfinUser
{
    [JsonPropertyName("Name")]
    public required string Name { get; init; }

    [JsonPropertyName("ServerId")]
    public required string ServerId { get; init; }

    [JsonPropertyName("Id")]
    public required string Id { get; init; }

    [JsonPropertyName("HasPassword")]
    public bool HasPassword { get; init; } = true;

    [JsonPropertyName("HasConfiguredPassword")]
    public bool HasConfiguredPassword { get; init; } = true;

    [JsonPropertyName("HasConfiguredEasyPassword")]
    public bool HasConfiguredEasyPassword { get; init; }

    [JsonPropertyName("EnableAutoLogin")]
    public bool EnableAutoLogin { get; init; }

    [JsonPropertyName("LastLoginDate")]
    public string? LastLoginDate { get; init; }

    [JsonPropertyName("LastActivityDate")]
    public string? LastActivityDate { get; init; }

    [JsonPropertyName("Configuration")]
    public JellyfinUserConfiguration? Configuration { get; init; }

    [JsonPropertyName("Policy")]
    public JellyfinUserPolicy? Policy { get; init; }
}

public record JellyfinUserConfiguration
{
    [JsonPropertyName("PlayDefaultAudioTrack")]
    public bool PlayDefaultAudioTrack { get; init; } = true;

    [JsonPropertyName("SubtitleLanguagePreference")]
    public string? SubtitleLanguagePreference { get; init; }

    [JsonPropertyName("DisplayMissingEpisodes")]
    public bool DisplayMissingEpisodes { get; init; }

    [JsonPropertyName("GroupedFolders")]
    public string[]? GroupedFolders { get; init; }

    [JsonPropertyName("SubtitleMode")]
    public string SubtitleMode { get; init; } = "Default";

    [JsonPropertyName("DisplayCollectionsView")]
    public bool DisplayCollectionsView { get; init; }

    [JsonPropertyName("EnableLocalPassword")]
    public bool EnableLocalPassword { get; init; }

    [JsonPropertyName("OrderedViews")]
    public string[]? OrderedViews { get; init; }

    [JsonPropertyName("LatestItemsExcludes")]
    public string[]? LatestItemsExcludes { get; init; }

    [JsonPropertyName("MyMediaExcludes")]
    public string[]? MyMediaExcludes { get; init; }

    [JsonPropertyName("HidePlayedInLatest")]
    public bool HidePlayedInLatest { get; init; } = true;

    [JsonPropertyName("RememberAudioSelections")]
    public bool RememberAudioSelections { get; init; } = true;

    [JsonPropertyName("RememberSubtitleSelections")]
    public bool RememberSubtitleSelections { get; init; } = true;

    [JsonPropertyName("EnableNextEpisodeAutoPlay")]
    public bool EnableNextEpisodeAutoPlay { get; init; } = true;
}

public record JellyfinUserPolicy
{
    [JsonPropertyName("IsAdministrator")]
    public bool IsAdministrator { get; init; }

    [JsonPropertyName("IsHidden")]
    public bool IsHidden { get; init; }

    [JsonPropertyName("IsDisabled")]
    public bool IsDisabled { get; init; }

    [JsonPropertyName("EnableUserPreferenceAccess")]
    public bool EnableUserPreferenceAccess { get; init; } = true;

    [JsonPropertyName("EnableRemoteControlOfOtherUsers")]
    public bool EnableRemoteControlOfOtherUsers { get; init; }

    [JsonPropertyName("EnableSharedDeviceControl")]
    public bool EnableSharedDeviceControl { get; init; } = true;

    [JsonPropertyName("EnableRemoteAccess")]
    public bool EnableRemoteAccess { get; init; } = true;

    [JsonPropertyName("EnableLiveTvManagement")]
    public bool EnableLiveTvManagement { get; init; }

    [JsonPropertyName("EnableLiveTvAccess")]
    public bool EnableLiveTvAccess { get; init; }

    [JsonPropertyName("EnableMediaPlayback")]
    public bool EnableMediaPlayback { get; init; } = true;

    [JsonPropertyName("EnableAudioPlaybackTranscoding")]
    public bool EnableAudioPlaybackTranscoding { get; init; } = true;

    [JsonPropertyName("EnableVideoPlaybackTranscoding")]
    public bool EnableVideoPlaybackTranscoding { get; init; }

    [JsonPropertyName("EnablePlaybackRemuxing")]
    public bool EnablePlaybackRemuxing { get; init; } = true;

    [JsonPropertyName("EnableContentDeletion")]
    public bool EnableContentDeletion { get; init; }

    [JsonPropertyName("EnableContentDownloading")]
    public bool EnableContentDownloading { get; init; } = true;

    [JsonPropertyName("EnableSyncTranscoding")]
    public bool EnableSyncTranscoding { get; init; } = true;

    [JsonPropertyName("EnableMediaConversion")]
    public bool EnableMediaConversion { get; init; }

    [JsonPropertyName("EnableAllChannels")]
    public bool EnableAllChannels { get; init; } = true;

    [JsonPropertyName("EnableAllFolders")]
    public bool EnableAllFolders { get; init; } = true;

    [JsonPropertyName("InvalidLoginAttemptCount")]
    public int InvalidLoginAttemptCount { get; init; }

    [JsonPropertyName("EnablePublicSharing")]
    public bool EnablePublicSharing { get; init; } = true;

    [JsonPropertyName("RemoteClientBitrateLimit")]
    public int RemoteClientBitrateLimit { get; init; }

    [JsonPropertyName("AuthenticationProviderId")]
    public string AuthenticationProviderId { get; init; } = "Melodee.Blazor.Auth.DefaultAuthenticationProvider";

    [JsonPropertyName("PasswordResetProviderId")]
    public string PasswordResetProviderId { get; init; } = "Melodee.Blazor.Auth.DefaultPasswordResetProvider";
}

public record JellyfinSessionInfo
{
    [JsonPropertyName("PlayState")]
    public JellyfinPlayState? PlayState { get; init; }

    [JsonPropertyName("Capabilities")]
    public JellyfinClientCapabilities? Capabilities { get; init; }

    [JsonPropertyName("RemoteEndPoint")]
    public string? RemoteEndPoint { get; init; }

    [JsonPropertyName("PlayableMediaTypes")]
    public string[]? PlayableMediaTypes { get; init; }

    [JsonPropertyName("Id")]
    public required string Id { get; init; }

    [JsonPropertyName("UserId")]
    public required string UserId { get; init; }

    [JsonPropertyName("UserName")]
    public string? UserName { get; init; }

    [JsonPropertyName("Client")]
    public string? Client { get; init; }

    [JsonPropertyName("LastActivityDate")]
    public string? LastActivityDate { get; init; }

    [JsonPropertyName("LastPlaybackCheckIn")]
    public string? LastPlaybackCheckIn { get; init; }

    [JsonPropertyName("DeviceName")]
    public string? DeviceName { get; init; }

    [JsonPropertyName("DeviceId")]
    public string? DeviceId { get; init; }

    [JsonPropertyName("ApplicationVersion")]
    public string? ApplicationVersion { get; init; }

    [JsonPropertyName("IsActive")]
    public bool IsActive { get; init; } = true;

    [JsonPropertyName("SupportsMediaControl")]
    public bool SupportsMediaControl { get; init; }

    [JsonPropertyName("SupportsRemoteControl")]
    public bool SupportsRemoteControl { get; init; }

    [JsonPropertyName("NowPlayingQueue")]
    public object[]? NowPlayingQueue { get; init; }

    [JsonPropertyName("NowPlayingQueueFullItems")]
    public object[]? NowPlayingQueueFullItems { get; init; }

    [JsonPropertyName("HasCustomDeviceName")]
    public bool HasCustomDeviceName { get; init; }

    [JsonPropertyName("ServerId")]
    public string? ServerId { get; init; }

    [JsonPropertyName("SupportedCommands")]
    public string[]? SupportedCommands { get; init; }
}

public record JellyfinPlayState
{
    [JsonPropertyName("CanSeek")]
    public bool CanSeek { get; init; }

    [JsonPropertyName("IsPaused")]
    public bool IsPaused { get; init; }

    [JsonPropertyName("IsMuted")]
    public bool IsMuted { get; init; }

    [JsonPropertyName("RepeatMode")]
    public string RepeatMode { get; init; } = "RepeatNone";
}

public record JellyfinClientCapabilities
{
    [JsonPropertyName("PlayableMediaTypes")]
    public string[]? PlayableMediaTypes { get; init; }

    [JsonPropertyName("SupportedCommands")]
    public string[]? SupportedCommands { get; init; }

    [JsonPropertyName("SupportsMediaControl")]
    public bool SupportsMediaControl { get; init; }

    [JsonPropertyName("SupportsContentUploading")]
    public bool SupportsContentUploading { get; init; }

    [JsonPropertyName("SupportsPersistentIdentifier")]
    public bool SupportsPersistentIdentifier { get; init; } = true;

    [JsonPropertyName("SupportsSync")]
    public bool SupportsSync { get; init; }
}

public record JellyfinAuthenticationResult
{
    [JsonPropertyName("User")]
    public required JellyfinUser User { get; init; }

    [JsonPropertyName("SessionInfo")]
    public JellyfinSessionInfo? SessionInfo { get; init; }

    [JsonPropertyName("AccessToken")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("ServerId")]
    public required string ServerId { get; init; }
}
