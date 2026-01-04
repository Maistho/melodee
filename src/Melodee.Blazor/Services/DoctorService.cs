using System.Diagnostics;
using System.Net.Sockets;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Blazor.Services;

/// <summary>
/// Service for performing system health checks and diagnostics.
/// </summary>
public sealed class DoctorService(
    IConfiguration configuration,
    IDbContextFactory<MelodeeDbContext> dbContextFactory,
    IDbContextFactory<MusicBrainzDbContext> musicBrainzDbContextFactory,
    IDbContextFactory<ArtistSearchEngineServiceDbContext> artistSearchEngineDbContextFactory,
    LibraryService libraryService,
    IWebHostEnvironment webHostEnvironment,
    IHttpContextAccessor httpContextAccessor) : IDoctorService
{
    private static readonly string[] RequiredConnectionStrings =
    [
        "DefaultConnection",
        "MusicBrainzConnection",
        "ArtistSearchEngineConnection"
    ];

    private static readonly string[] RequiredEnvironmentVariables =
    [
        // Add any required environment variables here if needed
    ];

    private const long DiskSpaceWarningThresholdBytes = 10L * 1024 * 1024 * 1024; // 10 GB
    private const long DiskSpaceCriticalThresholdBytes = 1L * 1024 * 1024 * 1024; // 1 GB
    private const int MinJwtKeyLength = 64; // For HMAC-SHA512

    public async Task<bool> NeedsAttentionAsync(CancellationToken cancellationToken = default)
    {
        // Check critical configuration
        if (HasMissingConnectionStrings())
        {
            return true;
        }

        // Check main database connectivity
        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            if (!await db.Database.CanConnectAsync(cancellationToken))
            {
                return true;
            }
        }
        catch
        {
            return true;
        }

        // Check MusicBrainz database
        if (await IsMusicBrainzDatabaseEmptyAsync(cancellationToken))
        {
            return true;
        }

        // Check library paths
        if (await HasLibraryPathIssuesAsync(cancellationToken))
        {
            return true;
        }

        // Check disk space
        if (await HasDiskSpaceIssuesAsync(cancellationToken))
        {
            return true;
        }

        // Check library paths overlap
        if (await HasLibraryPathOverlapAsync(cancellationToken))
        {
            return true;
        }

        // Check search engine API keys
        if (await HasSearchEngineApiKeyIssuesAsync(cancellationToken))
        {
            return true;
        }

        // Check SMTP configuration if email is enabled
        if (await HasSmtpConfigurationIssuesAsync(cancellationToken))
        {
            return true;
        }

        // Check JWT token strength
        if (HasJwtTokenStrengthIssues())
        {
            return true;
        }

        // Check HTTPS in production
        if (HasHttpsIssues())
        {
            return true;
        }

        // Check for default admin password
        if (await HasDefaultAdminPasswordAsync(cancellationToken))
        {
            return true;
        }

        return false;
    }

    public async Task<bool> IsMusicBrainzDatabaseEmptyAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("MusicBrainzConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return true;
        }

        try
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            var dataSource = builder.DataSource;

            if (string.IsNullOrWhiteSpace(dataSource))
            {
                return true;
            }

            if (!File.Exists(dataSource))
            {
                return true;
            }

            var fileInfo = new FileInfo(dataSource);
            if (fileInfo.Length == 0)
            {
                return true;
            }

            await using var db = await musicBrainzDbContextFactory.CreateDbContextAsync(cancellationToken);
            if (!await db.Database.CanConnectAsync(cancellationToken))
            {
                return true;
            }

            try
            {
                var artistCount = await db.Artists.Take(1).CountAsync(cancellationToken);
                return artistCount == 0;
            }
            catch
            {
                return true;
            }
        }
        catch
        {
            return true;
        }
    }

    public async Task<DoctorCheckResults> RunAllChecksAsync(CancellationToken cancellationToken = default)
    {
        var checks = new List<DoctorCheckResult>();
        var libraryPaths = new List<LibraryPathResult>();
        var configurableServices = new List<ConfigurableServiceResult>();
        var serilogLogPaths = new List<SerilogLogPathInfo>();
        var connectionStrings = new List<ConnectionStringInfo>();
        var environmentVariables = new List<EnvironmentVariableInfo>();
        var diskSpaceInfo = new List<DiskSpaceInfo>();
        var searchEngineApiKeys = new List<SearchEngineApiKeyInfo>();

        // Run all checks
        checks.Add(await RunConfigurationCheckAsync(cancellationToken));
        checks.Add(await RunDatabaseCheckAsync(cancellationToken));
        
        var (musicBrainzCheck, isMusicBrainzEmpty) = await RunMusicBrainzDbCheckAsync(cancellationToken);
        checks.Add(musicBrainzCheck);
        
        checks.Add(await RunArtistSearchEngineDbCheckAsync(cancellationToken));
        
        var (libraryCheck, libPaths) = await RunLibraryPathsCheckAsync(cancellationToken);
        checks.Add(libraryCheck);
        libraryPaths.AddRange(libPaths);

        var (serilogCheck, logPaths) = RunSerilogCheckAsync();
        checks.Add(serilogCheck);
        serilogLogPaths.AddRange(logPaths);

        var (servicesCheck, services) = await RunConfigurableServicesCheckAsync(cancellationToken);
        checks.Add(servicesCheck);
        configurableServices.AddRange(services);

        // Disk space and path checks
        var (diskSpaceCheck, diskInfo) = await RunDiskSpaceCheckAsync(cancellationToken);
        checks.Add(diskSpaceCheck);
        diskSpaceInfo.AddRange(diskInfo);

        var (pathOverlapCheck, _) = await RunLibraryPathOverlapCheckAsync(cancellationToken);
        checks.Add(pathOverlapCheck);

        var (apiKeysCheck, apiKeys) = await RunSearchEngineApiKeysCheckAsync(cancellationToken);
        checks.Add(apiKeysCheck);
        searchEngineApiKeys.AddRange(apiKeys);

        // Security and configuration checks
        checks.Add(await RunSmtpConfigurationCheckAsync(cancellationToken));
        checks.Add(RunJwtTokenStrengthCheck());
        checks.Add(RunHttpsCheck());
        checks.Add(await RunDefaultAdminPasswordCheckAsync(cancellationToken));

        // Gather connection string info
        connectionStrings.AddRange(GatherConnectionStringInfo());

        // Gather environment variable info
        environmentVariables.AddRange(GatherEnvironmentVariableInfo());

        return new DoctorCheckResults
        {
            Checks = checks,
            LibraryPaths = libraryPaths,
            ConfigurableServices = configurableServices,
            SerilogLogPaths = serilogLogPaths,
            ConnectionStrings = connectionStrings,
            EnvironmentVariables = environmentVariables,
            DiskSpaceInfo = diskSpaceInfo,
            SearchEngineApiKeys = searchEngineApiKeys,
            IsMusicBrainzEmpty = isMusicBrainzEmpty
        };
    }

    private bool HasMissingConnectionStrings()
    {
        foreach (var connStr in RequiredConnectionStrings)
        {
            if (string.IsNullOrWhiteSpace(configuration.GetConnectionString(connStr)))
            {
                return true;
            }
        }
        return false;
    }

    private async Task<bool> HasLibraryPathIssuesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var libs = await libraryService.ListAsync(new PagedRequest { PageSize = short.MaxValue }, cancellationToken);
            if (!libs.IsSuccess)
            {
                return true;
            }

            foreach (var lib in libs.Data)
            {
                if (!Directory.Exists(lib.Path))
                {
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return true;
        }
    }

    private async Task<DoctorCheckResult> RunConfigurationCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var missing = RequiredConnectionStrings
                .Where(cs => string.IsNullOrWhiteSpace(configuration.GetConnectionString(cs)))
                .ToList();

            var success = missing.Count == 0;
            var details = success
                ? $"Environment={webHostEnvironment.EnvironmentName}"
                : $"Missing: {string.Join(", ", missing)}";

            return new DoctorCheckResult("Configuration", success, details, sw.Elapsed);
        }
        catch (Exception ex)
        {
            return new DoctorCheckResult("Configuration", false, ex.Message, sw.Elapsed);
        }
    }

    private async Task<DoctorCheckResult> RunDatabaseCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var canConnect = await db.Database.CanConnectAsync(cancellationToken);
            var details = canConnect
                ? $"OK ({db.Database.ProviderName})"
                : "Unable to connect";

            return new DoctorCheckResult("PostgresDatabase", canConnect, details, sw.Elapsed);
        }
        catch (Exception ex)
        {
            return new DoctorCheckResult("PostgresDatabase", false, ex.Message, sw.Elapsed);
        }
    }

    private async Task<(DoctorCheckResult Check, bool IsEmpty)> RunMusicBrainzDbCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var connectionString = configuration.GetConnectionString("MusicBrainzConnection") ?? "";
            var fileInfo = DescribeSqlitePath(connectionString);

            var isEmpty = await IsMusicBrainzDatabaseEmptyAsync(cancellationToken);

            if (isEmpty)
            {
                return (new DoctorCheckResult("MusicBrainzDatabase", false, "MusicBrainz database is empty or not initialized", sw.Elapsed), true);
            }

            await using var db = await musicBrainzDbContextFactory.CreateDbContextAsync(cancellationToken);
            var canConnect = await db.Database.CanConnectAsync(cancellationToken);
            var details = canConnect ? $"OK; {fileInfo}" : $"Unable to connect; {fileInfo}";

            return (new DoctorCheckResult("MusicBrainzDatabase", canConnect, details, sw.Elapsed), false);
        }
        catch (Exception ex)
        {
            return (new DoctorCheckResult("MusicBrainzDatabase", false, ex.Message, sw.Elapsed), true);
        }
    }

    private async Task<DoctorCheckResult> RunArtistSearchEngineDbCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var connectionString = configuration.GetConnectionString("ArtistSearchEngineConnection") ?? "";
            var fileInfo = DescribeSqlitePath(connectionString);

            await using var db = await artistSearchEngineDbContextFactory.CreateDbContextAsync(cancellationToken);
            var canConnect = await db.Database.CanConnectAsync(cancellationToken);
            var details = canConnect ? $"OK; {fileInfo}" : $"Unable to connect; {fileInfo}";

            return new DoctorCheckResult("ArtistSearchEngineDatabase", canConnect, details, sw.Elapsed);
        }
        catch (Exception ex)
        {
            return new DoctorCheckResult("ArtistSearchEngineDatabase", false, ex.Message, sw.Elapsed);
        }
    }

    private async Task<(DoctorCheckResult Check, List<LibraryPathResult> Paths)> RunLibraryPathsCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var paths = new List<LibraryPathResult>();

        try
        {
            var libs = await libraryService.ListAsync(new PagedRequest { PageSize = short.MaxValue }, cancellationToken);
            if (!libs.IsSuccess)
            {
                return (new DoctorCheckResult("LibraryPaths", false, libs.Messages?.FirstOrDefault() ?? "Failed to list libraries", sw.Elapsed), paths);
            }

            foreach (var lib in libs.Data)
            {
                var exists = Directory.Exists(lib.Path);
                var writable = exists && IsDirectoryWritable(lib.Path);
                var details = GetLibraryPathDetails(exists, writable);

                paths.Add(new LibraryPathResult(
                    lib.Name,
                    lib.TypeValue.ToString(),
                    lib.Path,
                    exists,
                    writable,
                    details));
            }

            var anyMissing = paths.Any(l => !l.Exists);
            var anyNotWritable = paths.Any(l => l.Exists && !l.Writable);
            var hasIssues = anyMissing || anyNotWritable;

            var detailMessage = (anyMissing, anyNotWritable) switch
            {
                (true, true) => "Some paths missing and some not writable",
                (true, false) => "Some paths missing",
                (false, true) => "Some paths not writable",
                _ => "All library paths OK"
            };

            return (new DoctorCheckResult("LibraryPaths", !hasIssues, detailMessage, sw.Elapsed), paths);
        }
        catch (Exception ex)
        {
            return (new DoctorCheckResult("LibraryPaths", false, ex.Message, sw.Elapsed), paths);
        }
    }

    private (DoctorCheckResult Check, List<SerilogLogPathInfo> Paths) RunSerilogCheckAsync()
    {
        var sw = Stopwatch.StartNew();
        var paths = new List<SerilogLogPathInfo>();

        try
        {
            var filePaths = GetSerilogFilePathsFromConfig();

            foreach (var (sinkName, path) in filePaths)
            {
                var directoryPath = Path.GetDirectoryName(path);

                if (string.IsNullOrEmpty(directoryPath))
                {
                    var lastSeparator = path.LastIndexOfAny(['/', '\\']);
                    if (lastSeparator > 0)
                    {
                        directoryPath = path[..lastSeparator];
                    }
                }

                if (!string.IsNullOrEmpty(directoryPath))
                {
                    if (!Path.IsPathRooted(directoryPath))
                    {
                        directoryPath = Path.GetFullPath(Path.Combine(webHostEnvironment.ContentRootPath, directoryPath));
                    }

                    var dirExists = Directory.Exists(directoryPath);
                    var writable = dirExists && IsDirectoryWritable(directoryPath);

                    paths.Add(new SerilogLogPathInfo(sinkName, directoryPath, dirExists, writable));
                }
            }

            var hasIssues = paths.Any(p => !p.DirectoryExists || !p.Writable);
            var detailMessage = paths.Count == 0
                ? "No file logging configured"
                : hasIssues
                    ? "Some log paths have issues"
                    : "All log paths OK";

            return (new DoctorCheckResult("SerilogLogging", !hasIssues || paths.Count == 0, detailMessage, sw.Elapsed), paths);
        }
        catch (Exception ex)
        {
            return (new DoctorCheckResult("SerilogLogging", false, ex.Message, sw.Elapsed), paths);
        }
    }

    private async Task<(DoctorCheckResult Check, List<ConfigurableServiceResult> Services)> RunConfigurableServicesCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var services = new List<ConfigurableServiceResult>();

        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var settingsDict = await db.Settings
                .Where(s => s.Key.Contains(".enabled"))
                .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

            var serviceDefinitions = new (string Category, string Name, string SettingKey)[]
            {
                ("Search Engine", "Brave", "searchengine.brave.enabled"),
                ("Search Engine", "Deezer", "searchengine.deezer.enabled"),
                ("Search Engine", "iTunes", "searchengine.itunes.enabled"),
                ("Search Engine", "Last.fm", "searchengine.lastfm.enabled"),
                ("Search Engine", "MusicBrainz", "searchengine.musicbrainz.enabled"),
                ("Search Engine", "Spotify", "searchengine.spotify.enabled"),
                ("Search Engine", "Metal API", "searchengine.metalapi.enabled"),
                ("Scrobbling", "Scrobbling", "scrobbling.enabled"),
                ("Scrobbling", "Last.fm", "scrobbling.lastfm.enabled"),
                ("Processing", "Conversion", "conversion.enabled"),
                ("Processing", "Magic", "magic.enabled"),
                ("Processing", "Scripting", "scripting.enabled"),
                ("Plugins", "CueSheet", "plugin.cuesheet.enabled"),
                ("Plugins", "M3U", "plugin.m3u.enabled"),
                ("Plugins", "NFO", "plugin.nfo.enabled"),
                ("Plugins", "Simple File Verification", "plugin.sfv.enabled"),
                ("System", "Email", "email.enabled"),
            };

            foreach (var (category, name, settingKey) in serviceDefinitions)
            {
                var enabled = settingsDict.TryGetValue(settingKey, out var value)
                    && bool.TryParse(value, out var b) && b;
                services.Add(new ConfigurableServiceResult(category, name, settingKey, enabled));
            }

            var enabledCount = services.Count(s => s.Enabled);
            return (new DoctorCheckResult("ConfigurableServices", true, $"{enabledCount} of {services.Count} services enabled", sw.Elapsed), services);
        }
        catch (Exception ex)
        {
            return (new DoctorCheckResult("ConfigurableServices", false, ex.Message, sw.Elapsed), services);
        }
    }

    private List<ConnectionStringInfo> GatherConnectionStringInfo()
    {
        var results = new List<ConnectionStringInfo>();

        foreach (var name in RequiredConnectionStrings)
        {
            var value = configuration.GetConnectionString(name) ?? "";
            var isValid = !string.IsNullOrWhiteSpace(value);
            var isFileBased = value.Contains("Data Source=") && !value.Contains("Host=");
            bool? fileExists = null;
            bool? fileWritable = null;
            string? filePath = null;

            if (isFileBased && isValid)
            {
                try
                {
                    var builder = new SqliteConnectionStringBuilder(value);
                    filePath = builder.DataSource;
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        fileExists = File.Exists(filePath);
                        if (fileExists == true)
                        {
                            var dir = Path.GetDirectoryName(filePath);
                            fileWritable = !string.IsNullOrEmpty(dir) && IsDirectoryWritable(dir);
                        }
                    }
                }
                catch
                {
                    // Ignore parsing errors
                }
            }

            results.Add(new ConnectionStringInfo(
                name,
                MaskConnectionString(value),
                isValid,
                isFileBased,
                fileExists,
                fileWritable,
                filePath));
        }

        return results;
    }

    private List<EnvironmentVariableInfo> GatherEnvironmentVariableInfo()
    {
        var results = new List<EnvironmentVariableInfo>();

        foreach (var name in RequiredEnvironmentVariables)
        {
            var value = Environment.GetEnvironmentVariable(name) ?? "";
            var isSet = !string.IsNullOrWhiteSpace(value);
            results.Add(new EnvironmentVariableInfo(name, isSet ? MaskConnectionString(value) : "(not set)", isSet));
        }

        return results;
    }

    private static string DescribeSqlitePath(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return "No connection string";
        }

        try
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            var path = builder.DataSource;
            if (string.IsNullOrEmpty(path))
            {
                return "No data source in connection string";
            }

            if (!File.Exists(path))
            {
                return $"File not found: {path}";
            }

            var fi = new FileInfo(path);
            return $"{path} ({FormatFileSize(fi.Length)})";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        var order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }

    private static bool IsDirectoryWritable(string path)
    {
        try
        {
            var testFile = Path.Combine(path, $".melodee-write-test-{Guid.NewGuid():N}");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetLibraryPathDetails(bool exists, bool writable)
    {
        return (exists, writable) switch
        {
            (false, _) => "Path missing",
            (true, false) => "Path not writable",
            (true, true) => "OK"
        };
    }

    private static string MaskConnectionString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "(empty)";
        }

        if (value.Length <= 10)
        {
            return new string('*', value.Length);
        }

        return $"{value[..5]}...{value[^5..]}";
    }

    private List<(string SinkName, string Path)> GetSerilogFilePathsFromConfig()
    {
        var results = new List<(string SinkName, string Path)>();

        try
        {
            var writeToSection = configuration.GetSection("Serilog:WriteTo");
            var sinks = writeToSection.GetChildren().ToList();

            foreach (var sink in sinks)
            {
                var sinkName = sink.GetValue<string>("Name") ?? "Unknown";

                if (sinkName.Equals("File", StringComparison.OrdinalIgnoreCase))
                {
                    var path = sink.GetValue<string>("Args:path");
                    if (!string.IsNullOrEmpty(path))
                    {
                        results.Add((sinkName, path));
                    }
                }
            }
        }
        catch
        {
            // Ignore config parsing errors
        }

        return results;
    }

    private async Task<bool> HasDiskSpaceIssuesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var libs = await libraryService.ListAsync(new PagedRequest { PageSize = short.MaxValue }, cancellationToken);
            if (!libs.IsSuccess)
            {
                return false;
            }

            foreach (var lib in libs.Data)
            {
                if (!Directory.Exists(lib.Path))
                {
                    continue;
                }

                try
                {
                    var driveInfo = new DriveInfo(Path.GetPathRoot(lib.Path) ?? lib.Path);
                    if (driveInfo.AvailableFreeSpace < DiskSpaceCriticalThresholdBytes)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Ignore drive info errors
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> HasLibraryPathOverlapAsync(CancellationToken cancellationToken)
    {
        try
        {
            var libs = await libraryService.ListAsync(new PagedRequest { PageSize = short.MaxValue }, cancellationToken);
            if (!libs.IsSuccess || libs.Data.Count() < 2)
            {
                return false;
            }

            var paths = libs.Data
                .Select(l => NormalizePath(l.Path))
                .ToList();

            for (var i = 0; i < paths.Count; i++)
            {
                for (var j = i + 1; j < paths.Count; j++)
                {
                    if (IsPathOverlap(paths[i], paths[j]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> HasSearchEngineApiKeyIssuesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            
            var relevantKeys = new[]
            {
                SettingRegistry.SearchEngineSpotifyEnabled,
                SettingRegistry.SearchEngineSpotifyApiKey,
                SettingRegistry.SearchEngineSpotifyClientSecret
            };

            var settings = await db.Settings
                .Where(s => relevantKeys.Contains(s.Key))
                .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

            var spotifyEnabled = settings.TryGetValue(SettingRegistry.SearchEngineSpotifyEnabled, out var se) 
                && bool.TryParse(se, out var seb) && seb;
            var spotifyApiKey = settings.TryGetValue(SettingRegistry.SearchEngineSpotifyApiKey, out var sak) ? sak : "";
            var spotifySecret = settings.TryGetValue(SettingRegistry.SearchEngineSpotifyClientSecret, out var ss) ? ss : "";

            if (spotifyEnabled && (string.IsNullOrWhiteSpace(spotifyApiKey) || string.IsNullOrWhiteSpace(spotifySecret)))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<(DoctorCheckResult Check, List<DiskSpaceInfo> Info)> RunDiskSpaceCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var diskInfo = new List<DiskSpaceInfo>();

        try
        {
            var libs = await libraryService.ListAsync(new PagedRequest { PageSize = short.MaxValue }, cancellationToken);
            if (!libs.IsSuccess)
            {
                return (new DoctorCheckResult("DiskSpace", false, "Failed to list libraries", sw.Elapsed), diskInfo);
            }

            var checkedRoots = new HashSet<string>();

            foreach (var lib in libs.Data)
            {
                if (!Directory.Exists(lib.Path))
                {
                    continue;
                }

                try
                {
                    var root = Path.GetPathRoot(lib.Path) ?? lib.Path;
                    
                    if (checkedRoots.Contains(root))
                    {
                        continue;
                    }
                    checkedRoots.Add(root);

                    var driveInfo = new DriveInfo(root);
                    var availableBytes = driveInfo.AvailableFreeSpace;
                    var totalBytes = driveInfo.TotalSize;
                    var usedBytes = totalBytes - availableBytes;
                    var usedPercent = totalBytes > 0 ? (double)usedBytes / totalBytes * 100 : 0;

                    var status = availableBytes switch
                    {
                        < DiskSpaceCriticalThresholdBytes => DiskSpaceStatus.Critical,
                        < DiskSpaceWarningThresholdBytes => DiskSpaceStatus.Warning,
                        _ => DiskSpaceStatus.Ok
                    };

                    diskInfo.Add(new DiskSpaceInfo(
                        lib.Name,
                        root,
                        totalBytes,
                        availableBytes,
                        usedBytes,
                        usedPercent,
                        status));
                }
                catch
                {
                    diskInfo.Add(new DiskSpaceInfo(
                        lib.Name,
                        lib.Path,
                        0,
                        0,
                        0,
                        0,
                        DiskSpaceStatus.Unknown));
                }
            }

            var hasCritical = diskInfo.Any(d => d.Status == DiskSpaceStatus.Critical);
            var hasWarning = diskInfo.Any(d => d.Status == DiskSpaceStatus.Warning);

            var detailMessage = (hasCritical, hasWarning) switch
            {
                (true, _) => "Critical: Low disk space on one or more drives",
                (false, true) => "Warning: Disk space is getting low on one or more drives",
                _ => "All drives have adequate space"
            };

            return (new DoctorCheckResult("DiskSpace", !hasCritical, detailMessage, sw.Elapsed), diskInfo);
        }
        catch (Exception ex)
        {
            return (new DoctorCheckResult("DiskSpace", false, ex.Message, sw.Elapsed), diskInfo);
        }
    }

    private async Task<(DoctorCheckResult Check, List<string> Overlaps)> RunLibraryPathOverlapCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var overlaps = new List<string>();

        try
        {
            var libs = await libraryService.ListAsync(new PagedRequest { PageSize = short.MaxValue }, cancellationToken);
            if (!libs.IsSuccess)
            {
                return (new DoctorCheckResult("LibraryPathOverlap", false, "Failed to list libraries", sw.Elapsed), overlaps);
            }

            var libList = libs.Data.ToList();

            for (var i = 0; i < libList.Count; i++)
            {
                for (var j = i + 1; j < libList.Count; j++)
                {
                    var path1 = NormalizePath(libList[i].Path);
                    var path2 = NormalizePath(libList[j].Path);

                    if (IsPathOverlap(path1, path2))
                    {
                        overlaps.Add($"{libList[i].Name} ({path1}) overlaps with {libList[j].Name} ({path2})");
                    }
                }
            }

            var hasOverlaps = overlaps.Count > 0;
            var detailMessage = hasOverlaps
                ? $"Warning: {overlaps.Count} library path overlap(s) detected"
                : "No library path overlaps detected";

            return (new DoctorCheckResult("LibraryPathOverlap", !hasOverlaps, detailMessage, sw.Elapsed), overlaps);
        }
        catch (Exception ex)
        {
            return (new DoctorCheckResult("LibraryPathOverlap", false, ex.Message, sw.Elapsed), overlaps);
        }
    }

    private async Task<(DoctorCheckResult Check, List<SearchEngineApiKeyInfo> ApiKeys)> RunSearchEngineApiKeysCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var apiKeys = new List<SearchEngineApiKeyInfo>();

        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var searchEngineDefinitions = new (string Name, string EnabledKey, string? ApiKeyKey, string? SecretKey)[]
            {
                ("Spotify", SettingRegistry.SearchEngineSpotifyEnabled, SettingRegistry.SearchEngineSpotifyApiKey, SettingRegistry.SearchEngineSpotifyClientSecret),
                ("Brave", SettingRegistry.SearchEngineBraveEnabled, SettingRegistry.SearchEngineBraveApiKey, null),
                ("Last.fm (Scrobbling)", SettingRegistry.ScrobblingLastFmEnabled, SettingRegistry.ScrobblingLastFmApiKey, SettingRegistry.ScrobblingLastFmSharedSecret),
            };

            var allKeys = searchEngineDefinitions
                .SelectMany(d => new[] { d.EnabledKey, d.ApiKeyKey, d.SecretKey })
                .Where(k => k != null)
                .Cast<string>()
                .ToList();

            var settings = await db.Settings
                .Where(s => allKeys.Contains(s.Key))
                .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

            foreach (var (name, enabledKey, apiKeyKey, secretKey) in searchEngineDefinitions)
            {
                var isEnabled = settings.TryGetValue(enabledKey, out var enabledValue) 
                    && bool.TryParse(enabledValue, out var eb) && eb;

                var hasApiKey = apiKeyKey != null 
                    && settings.TryGetValue(apiKeyKey, out var apiKeyValue) 
                    && !string.IsNullOrWhiteSpace(apiKeyValue);

                var hasSecret = secretKey == null 
                    || (settings.TryGetValue(secretKey, out var secretValue) 
                        && !string.IsNullOrWhiteSpace(secretValue));

                var isConfigured = hasApiKey && hasSecret;

                var status = (isEnabled, isConfigured) switch
                {
                    (true, true) => "Enabled and configured",
                    (true, false) => "⚠️ Enabled but missing API key/secret",
                    (false, true) => "Disabled (API key configured)",
                    (false, false) => "Disabled"
                };

                apiKeys.Add(new SearchEngineApiKeyInfo(
                    name,
                    enabledKey,
                    isEnabled,
                    isConfigured,
                    status));
            }

            var hasIssues = apiKeys.Any(k => k.IsEnabled && !k.IsConfigured);
            var detailMessage = hasIssues
                ? "Some enabled search engines are missing API keys"
                : "All enabled search engines are properly configured";

            return (new DoctorCheckResult("SearchEngineApiKeys", !hasIssues, detailMessage, sw.Elapsed), apiKeys);
        }
        catch (Exception ex)
        {
            return (new DoctorCheckResult("SearchEngineApiKeys", false, ex.Message, sw.Elapsed), apiKeys);
        }
    }

    private static string NormalizePath(string path)
    {
        path = Path.GetFullPath(path);
        if (!path.EndsWith(Path.DirectorySeparatorChar))
        {
            path += Path.DirectorySeparatorChar;
        }
        return path.ToLowerInvariant();
    }

    private static bool IsPathOverlap(string path1, string path2)
    {
        return path1.StartsWith(path2) || path2.StartsWith(path1);
    }

    private async Task<bool> HasSmtpConfigurationIssuesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            
            var relevantKeys = new[]
            {
                SettingRegistry.EmailEnabled,
                SettingRegistry.EmailSmtpHost,
                SettingRegistry.EmailSmtpPort
            };

            var settings = await db.Settings
                .Where(s => relevantKeys.Contains(s.Key))
                .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

            var emailEnabled = settings.TryGetValue(SettingRegistry.EmailEnabled, out var ee) 
                && bool.TryParse(ee, out var eeb) && eeb;

            if (!emailEnabled)
            {
                return false;
            }

            var smtpHost = settings.TryGetValue(SettingRegistry.EmailSmtpHost, out var host) ? host : "";
            var smtpPort = settings.TryGetValue(SettingRegistry.EmailSmtpPort, out var port) ? port : "";

            return string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpPort);
        }
        catch
        {
            return false;
        }
    }

    private async Task<DoctorCheckResult> RunSmtpConfigurationCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var relevantKeys = new[]
            {
                SettingRegistry.EmailEnabled,
                SettingRegistry.EmailSmtpHost,
                SettingRegistry.EmailSmtpPort,
                SettingRegistry.EmailFromEmail
            };

            var settings = await db.Settings
                .Where(s => relevantKeys.Contains(s.Key))
                .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

            var emailEnabled = settings.TryGetValue(SettingRegistry.EmailEnabled, out var ee) 
                && bool.TryParse(ee, out var eeb) && eeb;

            if (!emailEnabled)
            {
                return new DoctorCheckResult("SmtpConfiguration", true, "Email is disabled", sw.Elapsed);
            }

            var smtpHost = settings.TryGetValue(SettingRegistry.EmailSmtpHost, out var host) ? host : "";
            var smtpPort = settings.TryGetValue(SettingRegistry.EmailSmtpPort, out var port) ? port : "";
            var fromEmail = settings.TryGetValue(SettingRegistry.EmailFromEmail, out var from) ? from : "";

            var issues = new List<string>();

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                issues.Add("SMTP host not configured");
            }

            if (string.IsNullOrWhiteSpace(smtpPort) || !int.TryParse(smtpPort, out _))
            {
                issues.Add("SMTP port not configured");
            }

            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                issues.Add("From email not configured");
            }

            if (issues.Count > 0)
            {
                return new DoctorCheckResult("SmtpConfiguration", false, $"Email enabled but: {string.Join(", ", issues)}", sw.Elapsed);
            }

            return new DoctorCheckResult("SmtpConfiguration", true, $"SMTP configured: {smtpHost}:{smtpPort}", sw.Elapsed);
        }
        catch (Exception ex)
        {
            return new DoctorCheckResult("SmtpConfiguration", false, ex.Message, sw.Elapsed);
        }
    }

    private bool HasJwtTokenStrengthIssues()
    {
        var jwtKey = configuration.GetValue<string>("Jwt:Key") ?? "";
        return jwtKey.Length < MinJwtKeyLength;
    }

    private DoctorCheckResult RunJwtTokenStrengthCheck()
    {
        var sw = Stopwatch.StartNew();

        try
        {
            // Check multiple configuration sources (supports container, non-container, and .env environments)
            // Priority order: Jwt:Key (appsettings/env) -> MelodeeAuthSettings:Token (appsettings/env) -> MELODEE_AUTH_TOKEN (env/.env)
            var jwtKey = configuration.GetValue<string>("Jwt:Key");
            
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                jwtKey = configuration.GetValue<string>("MelodeeAuthSettings:Token");
            }
            
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                jwtKey = Environment.GetEnvironmentVariable("MELODEE_AUTH_TOKEN");
            }

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                return new DoctorCheckResult("JwtTokenStrength", false, 
                    "JWT key is not configured (checked Jwt:Key, MelodeeAuthSettings:Token, and MELODEE_AUTH_TOKEN environment variable)", sw.Elapsed);
            }

            if (jwtKey.Length < MinJwtKeyLength)
            {
                return new DoctorCheckResult("JwtTokenStrength", false, 
                    $"JWT key too short ({jwtKey.Length} chars). Minimum {MinJwtKeyLength} chars required for HMAC-SHA512", sw.Elapsed);
            }

            return new DoctorCheckResult("JwtTokenStrength", true, $"JWT key length: {jwtKey.Length} chars (OK)", sw.Elapsed);
        }
        catch (Exception ex)
        {
            return new DoctorCheckResult("JwtTokenStrength", false, ex.Message, sw.Elapsed);
        }
    }

    private bool HasHttpsIssues()
    {
        if (!webHostEnvironment.IsProduction())
        {
            return false;
        }

        var httpContext = httpContextAccessor.HttpContext;
        return httpContext is { Request.IsHttps: false };
    }

    private DoctorCheckResult RunHttpsCheck()
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var isProduction = webHostEnvironment.IsProduction();
            var httpContext = httpContextAccessor.HttpContext;
            var isHttps = httpContext?.Request.IsHttps ?? true;

            if (!isProduction)
            {
                return new DoctorCheckResult("HttpsSecurity", true, $"Non-production environment ({webHostEnvironment.EnvironmentName})", sw.Elapsed);
            }

            if (isHttps)
            {
                return new DoctorCheckResult("HttpsSecurity", true, "HTTPS is enabled in production", sw.Elapsed);
            }

            return new DoctorCheckResult("HttpsSecurity", false, "⚠️ Running in production without HTTPS", sw.Elapsed);
        }
        catch (Exception ex)
        {
            return new DoctorCheckResult("HttpsSecurity", false, ex.Message, sw.Elapsed);
        }
    }

    private async Task<bool> HasDefaultAdminPasswordAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            
            var adminUser = await db.Users
                .Where(u => u.IsAdmin && u.UserNameNormalized == "ADMIN")
                .FirstOrDefaultAsync(cancellationToken);

            if (adminUser == null)
            {
                return false;
            }

            return adminUser.LastLoginAt == null && adminUser.LastActivityAt == null;
        }
        catch
        {
            return false;
        }
    }

    private async Task<DoctorCheckResult> RunDefaultAdminPasswordCheckAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var adminUser = await db.Users
                .Where(u => u.IsAdmin && u.UserNameNormalized == "ADMIN")
                .FirstOrDefaultAsync(cancellationToken);

            if (adminUser == null)
            {
                return new DoctorCheckResult("AdminPassword", true, "No default 'admin' user found", sw.Elapsed);
            }

            if (adminUser.LastLoginAt == null && adminUser.LastActivityAt == null)
            {
                return new DoctorCheckResult("AdminPassword", false, 
                    "⚠️ Default admin account has never logged in - consider changing the password", sw.Elapsed);
            }

            return new DoctorCheckResult("AdminPassword", true, "Admin account has been used", sw.Elapsed);
        }
        catch (Exception ex)
        {
            return new DoctorCheckResult("AdminPassword", false, ex.Message, sw.Elapsed);
        }
    }
}
