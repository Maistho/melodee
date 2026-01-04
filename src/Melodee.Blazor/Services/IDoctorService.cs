namespace Melodee.Blazor.Services;

/// <summary>
/// Service for performing system health checks and diagnostics.
/// </summary>
public interface IDoctorService
{
    /// <summary>
    /// Quickly checks if any critical issues need attention.
    /// Used by the Dashboard to show/hide the health warning banner.
    /// </summary>
    Task<bool> NeedsAttentionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs all diagnostic checks and returns detailed results.
    /// Used by the Doctor page to display comprehensive health information.
    /// </summary>
    Task<DoctorCheckResults> RunAllChecksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the MusicBrainz database is empty or not properly initialized.
    /// </summary>
    Task<bool> IsMusicBrainzDatabaseEmptyAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Results from running all doctor checks.
/// </summary>
public sealed record DoctorCheckResults
{
    public required IReadOnlyList<DoctorCheckResult> Checks { get; init; }
    public required IReadOnlyList<LibraryPathResult> LibraryPaths { get; init; }
    public required IReadOnlyList<ConfigurableServiceResult> ConfigurableServices { get; init; }
    public required IReadOnlyList<SerilogLogPathInfo> SerilogLogPaths { get; init; }
    public required IReadOnlyList<ConnectionStringInfo> ConnectionStrings { get; init; }
    public required IReadOnlyList<EnvironmentVariableInfo> EnvironmentVariables { get; init; }
    public required IReadOnlyList<DiskSpaceInfo> DiskSpaceInfo { get; init; }
    public required IReadOnlyList<SearchEngineApiKeyInfo> SearchEngineApiKeys { get; init; }
    
    public bool HasIssues => Checks.Any(c => !c.Success);
    public bool IsMusicBrainzEmpty { get; init; }
}

/// <summary>
/// Result of a single diagnostic check.
/// </summary>
public sealed record DoctorCheckResult(string Name, bool Success, string Details, TimeSpan Duration);

/// <summary>
/// Information about a library path.
/// </summary>
public sealed record LibraryPathResult(string Name, string Type, string Path, bool Exists, bool Writable, string Details);

/// <summary>
/// Information about a configurable service.
/// </summary>
public sealed record ConfigurableServiceResult(string Category, string Name, string SettingKey, bool Enabled);

/// <summary>
/// Information about a Serilog log path.
/// </summary>
public sealed record SerilogLogPathInfo(string SinkName, string Path, bool DirectoryExists, bool Writable);

/// <summary>
/// Information about a connection string.
/// </summary>
public sealed record ConnectionStringInfo(string Name, string MaskedValue, bool IsValid, bool IsFileBased, bool? FileExists, bool? FileWritable, string? FilePath);

/// <summary>
/// Information about an environment variable.
/// </summary>
public sealed record EnvironmentVariableInfo(string Name, string MaskedValue, bool IsSet);

/// <summary>
/// Information about disk space for a storage path.
/// </summary>
public sealed record DiskSpaceInfo(
    string Name,
    string Path,
    long TotalBytes,
    long AvailableBytes,
    long UsedBytes,
    double UsedPercent,
    DiskSpaceStatus Status);

/// <summary>
/// Status of disk space for a path.
/// </summary>
public enum DiskSpaceStatus
{
    Ok,
    Warning,
    Critical,
    Unknown
}

/// <summary>
/// Information about a search engine API key configuration.
/// </summary>
public sealed record SearchEngineApiKeyInfo(
    string EngineName,
    string SettingKey,
    bool IsEnabled,
    bool IsConfigured,
    string Status);
