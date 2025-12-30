using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.SearchEngines;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Cli.Command;

/// <summary>
/// Find duplicate album directories for artists and optionally resolve them using metadata searches.
/// </summary>
public class AlbumFindDuplicateDirsCommand : CommandBase<AlbumFindDuplicateDirsSettings>
{
    // Pre-compiled regex patterns for better performance
    private static readonly Regex YearRegex = new(@"\[(\d{4})\]|\((\d{4})\)|^(\d{4})\s", RegexOptions.Compiled);
    private static readonly Regex YearRemovalRegex = new(@"\s*[\[\(]?\d{4}[\]\)]?\s*", RegexOptions.Compiled);
    
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        AlbumFindDuplicateDirsSettings settings,
        CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();
        
        Log.Debug("Starting AlbumFindDuplicateDirsCommand with settings: LibraryName={LibraryName}, ArtistFilter={ArtistFilter}, SearchMetadata={SearchMetadata}, Delete={Delete}, Limit={Limit}",
            settings.LibraryName, settings.ArtistFilter, settings.SearchMetadata, settings.Delete, settings.Limit);

        using var scope = CreateServiceProvider().CreateScope();
        var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
        var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
        var configurationFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();
        var albumSearchEngineService = scope.ServiceProvider.GetRequiredService<AlbumSearchEngineService>();

        Log.Debug("Services resolved successfully");
        
        var libraries = await libraryService.ListAsync(new PagedRequest { PageSize = short.MaxValue }, cancellationToken);
        Log.Debug("Found {LibraryCount} libraries", libraries.Data.Count());
        
        var library = libraries.Data.FirstOrDefault(x => x.Name.ToNormalizedString() == settings.LibraryName?.ToNormalizedString());

        if (library == null)
        {
            Log.Error("Library '{LibraryName}' not found. Available libraries: {AvailableLibraries}", 
                settings.LibraryName, string.Join(", ", libraries.Data.Select(l => l.Name)));
            AnsiConsole.MarkupLine($"[red]Error:[/] Library '{settings.LibraryName}' not found.");
            return 1;
        }

        Log.Debug("Found library: Id={LibraryId}, Name={LibraryName}, Path={LibraryPath}, Type={LibraryType}",
            library.Id, library.Name, library.Path, library.TypeValue);

        if (library.TypeValue != LibraryType.Storage)
        {
            Log.Error("Library '{LibraryName}' is type '{LibraryType}', but Storage type is required",
                settings.LibraryName, library.TypeValue);
            AnsiConsole.MarkupLine($"[red]Error:[/] This command requires a Storage library. '{settings.LibraryName}' is type '{library.TypeValue}'.");
            return 1;
        }

        if (settings.Delete && !settings.SearchMetadata)
        {
            Log.Error("--delete flag requires --search flag to be set");
            AnsiConsole.MarkupLine("[red]Error:[/] --delete requires --search to determine which directory is correct.");
            return 1;
        }

        var duplicateGroups = new List<DuplicateAlbumGroup>();

        Log.Information("Starting scan of library path: {LibraryPath}", library.Path);
        var scanStartTime = Stopwatch.GetTimestamp();
        
        AnsiConsole.MarkupLine($"[blue]Scanning library:[/] {library.Path.EscapeMarkup()}");
        duplicateGroups = await FindDuplicateAlbumDirectoriesAsync(library.Path, settings.ArtistFilter, serializer, cancellationToken);
        
        var scanElapsed = Stopwatch.GetElapsedTime(scanStartTime);
        Log.Information("Directory scan completed in {ElapsedSeconds:F2}s. Found {GroupCount} duplicate groups", 
            scanElapsed.TotalSeconds, duplicateGroups.Count);

        if (settings.Limit.HasValue && settings.Limit.Value > 0)
        {
            Log.Debug("Applying limit of {Limit} groups", settings.Limit.Value);
            duplicateGroups = duplicateGroups.Take(settings.Limit.Value).ToList();
        }

        if (duplicateGroups.Count == 0)
        {
            Log.Information("No duplicate album directories found");
            AnsiConsole.MarkupLine("[green]No duplicate album directories found.[/]");
            return 0;
        }

        Log.Information("Processing {GroupCount} duplicate groups", duplicateGroups.Count);

        if (settings.SearchMetadata)
        {
            Log.Information("Initializing metadata search engine");
            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
            await albumSearchEngineService.InitializeAsync(configuration, cancellationToken);
            Log.Debug("Search engine initialized successfully");

            var searchStartTime = Stopwatch.GetTimestamp();
            var searchedCount = 0;
            var resolvedCount = 0;
            
            await AnsiConsole.Progress()
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn())
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask("[green]Searching metadata sources...[/]", maxValue: duplicateGroups.Count);

                    foreach (var group in duplicateGroups)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            Log.Warning("Metadata search cancelled by user");
                            break;
                        }

                        try
                        {
                            Log.Debug("Searching metadata for Artist={Artist}, Album={Album}", 
                                group.ArtistName, group.AlbumName);
                            
                            var searchResult = await albumSearchEngineService.DoSearchAsync(
                                new AlbumQuery
                                {
                                    Artist = group.ArtistName,
                                    Name = group.AlbumName,
                                    Year = 0
                                },
                                10,
                                cancellationToken);

                            searchedCount++;
                            Log.Debug("Search returned {ResultCount} results for {Artist} - {Album}", 
                                searchResult.Data.Count(), group.ArtistName, group.AlbumName);

                            if (searchResult.Data.Any())
                            {
                                var matchingAlbum = searchResult.Data
                                    .Where(a => a.NameNormalized == group.AlbumName.ToNormalizedString())
                                    .OrderByDescending(a => a.Rank)
                                    .FirstOrDefault();

                                if (matchingAlbum != null && matchingAlbum.Year > 0)
                                {
                                    Log.Debug("Found matching album: Year={Year}, Rank={Rank}, MusicBrainzId={MbId}, SpotifyId={SpotifyId}",
                                        matchingAlbum.Year, matchingAlbum.Rank, matchingAlbum.MusicBrainzId, matchingAlbum.SpotifyId);
                                    
                                    group.MetadataYear = matchingAlbum.Year;
                                    group.MetadataSource = "SearchEngine";
                                    group.MusicBrainzId = matchingAlbum.MusicBrainzId;
                                    group.SpotifyId = matchingAlbum.SpotifyId;

                                    foreach (var dir in group.Directories)
                                    {
                                        dir.IsCorrectYear = dir.Year == matchingAlbum.Year;
                                        Log.Debug("Directory {Path}: Year={Year}, IsCorrectYear={IsCorrect}",
                                            dir.Path, dir.Year, dir.IsCorrectYear);
                                    }

                                    group.SuggestedCorrectDirectory = group.Directories
                                        .FirstOrDefault(d => d.IsCorrectYear == true)?.Path;
                                    group.SuggestedDeleteDirectories = group.Directories
                                        .Where(d => d.IsCorrectYear == false)
                                        .Select(d => d.Path)
                                        .ToArray();
                                    
                                    resolvedCount++;
                                    Log.Debug("Resolved {Artist} - {Album}: CorrectDir={CorrectDir}, DeleteDirs={DeleteCount}",
                                        group.ArtistName, group.AlbumName, 
                                        group.SuggestedCorrectDirectory, group.SuggestedDeleteDirectories?.Length ?? 0);
                                }
                                else
                                {
                                    Log.Debug("No matching album with valid year found for {Artist} - {Album}",
                                        group.ArtistName, group.AlbumName);
                                }
                            }
                            else
                            {
                                Log.Debug("No search results for {Artist} - {Album}", group.ArtistName, group.AlbumName);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Failed to search metadata for {Artist} - {Album}", group.ArtistName, group.AlbumName);
                        }

                        task.Increment(1);
                    }
                });
            
            var searchElapsed = Stopwatch.GetElapsedTime(searchStartTime);
            Log.Information("Metadata search completed in {ElapsedSeconds:F2}s. Searched={Searched}, Resolved={Resolved}",
                searchElapsed.TotalSeconds, searchedCount, resolvedCount);
        }

        if (settings.JsonOutput)
        {
            Log.Debug("Outputting results as JSON");
            OutputJson(duplicateGroups, startTime);
        }
        else
        {
            Log.Debug("Outputting results as table");
            OutputTable(duplicateGroups, settings.SearchMetadata);
        }

        if (settings.Delete && settings.SearchMetadata)
        {
            Log.Information("Starting delete operation for duplicate directories");
            await DeleteDuplicateDirectoriesAsync(duplicateGroups, cancellationToken);
        }

        var totalElapsed = Stopwatch.GetElapsedTime(startTime);
        Log.Information("AlbumFindDuplicateDirsCommand completed in {ElapsedSeconds:F2}s", totalElapsed.TotalSeconds);

        return 0;
    }

    private static async Task<List<DuplicateAlbumGroup>> FindDuplicateAlbumDirectoriesAsync(
        string libraryPath,
        string? artistFilter,
        ISerializer serializer,
        CancellationToken cancellationToken)
    {
        var duplicateGroups = new List<DuplicateAlbumGroup>();
        var artistDirectories = new System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentBag<AlbumDirectoryInfo>>();

        Log.Debug("FindDuplicateAlbumDirectoriesAsync starting. LibraryPath={LibraryPath}, ArtistFilter={ArtistFilter}",
            libraryPath, artistFilter);

        if (!Directory.Exists(libraryPath))
        {
            Log.Error("Library path does not exist: {LibraryPath}", libraryPath);
            return duplicateGroups;
        }

        var enumerationOptions = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint | FileAttributes.System,
            ReturnSpecialDirectories = false
        };

        var topLevelDirs = Directory.EnumerateDirectories(libraryPath, "*", enumerationOptions)
            .Where(d => Path.GetFileName(d).Length == 1)
            .ToArray();
        Log.Debug("Found {TopLevelDirCount} letter directories in library", topLevelDirs.Length);
        
        var processedArtists = 0;
        var processedAlbums = 0;
        var scanStartTime = Stopwatch.GetTimestamp();

        // Collect all artist directories first (fast enumeration)
        var allArtistDirs = new List<(string artistDir, string artistName)>();
        
        foreach (var letterDir in topLevelDirs)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var letterName = Path.GetFileName(letterDir);
            
            foreach (var twoLetterDir in Directory.EnumerateDirectories(letterDir, "*", enumerationOptions))
            {
                var twoLetterName = Path.GetFileName(twoLetterDir);
                if (twoLetterName.Length != 2)
                {
                    continue;
                }

                foreach (var artistDir in Directory.EnumerateDirectories(twoLetterDir, "*", enumerationOptions))
                {
                    var artistName = Path.GetFileName(artistDir);

                    if (!string.IsNullOrWhiteSpace(artistFilter) &&
                        !artistName.Contains(artistFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    allArtistDirs.Add((artistDir, artistName));
                }
            }
            
            Log.Verbose("Enumerated letter '{Letter}': {ArtistCount} artists so far", letterName, allArtistDirs.Count);
        }

        var enumElapsed = Stopwatch.GetElapsedTime(scanStartTime);
        Log.Information("Directory enumeration completed in {ElapsedSeconds:F2}s. Found {ArtistCount} artist directories",
            enumElapsed.TotalSeconds, allArtistDirs.Count);
        
        AnsiConsole.MarkupLine($"[grey]Found {allArtistDirs.Count:N0} artist directories in {enumElapsed.TotalSeconds:F1}s[/]");

        // Process artist directories in parallel with limited concurrency
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
            CancellationToken = cancellationToken
        };

        var progressLock = new object();
        var lastProgressReport = Stopwatch.GetTimestamp();
        var duplicateCount = 0; // Track duplicates incrementally instead of recounting
        
        await AnsiConsole.Progress()
            .AutoRefresh(true)
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var artistTask = ctx.AddTask("[green]Scanning artists[/]", maxValue: allArtistDirs.Count);
                var albumTask = ctx.AddTask("[blue]Albums scanned[/]", maxValue: 1, autoStart: false);
                albumTask.IsIndeterminate = true;
                albumTask.StartTask();
                var duplicateTask = ctx.AddTask("[yellow]Duplicates found[/]", maxValue: 1, autoStart: false);
                duplicateTask.IsIndeterminate = true;
                duplicateTask.StartTask();
                
                await Parallel.ForEachAsync(allArtistDirs, parallelOptions, async (item, ct) =>
                {
                    var (artistDir, artistName) = item;
                    Interlocked.Increment(ref processedArtists);
                    
                    try
                    {
                        foreach (var albumDir in Directory.EnumerateDirectories(artistDir, "*", enumerationOptions))
                        {
                            // Combine file enumeration - get media files in one pass
                            var mediaFiles = Directory.EnumerateFiles(albumDir, "*", enumerationOptions)
                                .Where(f => Common.Utility.FileHelper.IsFileMediaType(Path.GetExtension(f)))
                                .ToList();

                            if (mediaFiles.Count == 0)
                            {
                                continue;
                            }

                            Interlocked.Increment(ref processedAlbums);
                            var albumInfo = ParseAlbumDirectory(albumDir, artistName, serializer, mediaFiles.Count, ct);
                            
                            if (albumInfo != null)
                            {
                                var key = $"{artistName}|{albumInfo.AlbumNameNormalized}";
                                var bag = artistDirectories.GetOrAdd(key, _ => new System.Collections.Concurrent.ConcurrentBag<AlbumDirectoryInfo>());
                                var previousCount = bag.Count;
                                bag.Add(albumInfo);
                                
                                // Track duplicates incrementally: if we just created a duplicate (count went from 1 to 2)
                                if (previousCount == 1)
                                {
                                    Interlocked.Increment(ref duplicateCount);
                                }
                            }
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        Log.Warning(ex, "Error processing artist directory: {ArtistDir}", artistDir);
                    }

                    // Update progress bar
                    artistTask.Increment(1);
                    
                    // Update descriptions with current counts periodically
                    var now = Stopwatch.GetTimestamp();
                    if (Stopwatch.GetElapsedTime(lastProgressReport, now).TotalMilliseconds >= 250)
                    {
                        lock (progressLock)
                        {
                            if (Stopwatch.GetElapsedTime(lastProgressReport, now).TotalMilliseconds >= 250)
                            {
                                lastProgressReport = now;
                                albumTask.Description = $"[blue]Albums scanned: {processedAlbums:N0}[/]";
                                duplicateTask.Description = $"[yellow]Duplicates found: {duplicateCount:N0}[/]";
                                
                                // Log to file every 5 seconds
                                var elapsed = Stopwatch.GetElapsedTime(scanStartTime);
                                if (elapsed.TotalSeconds % 5 < 0.3)
                                {
                                    Log.Information("Progress: Artists={Artists}/{Total}, Albums={Albums}, Duplicates={Dups}, Elapsed={Elapsed:F1}s",
                                        processedArtists, allArtistDirs.Count, processedAlbums, duplicateCount, elapsed.TotalSeconds);
                                }
                            }
                        }
                    }
                });
                
                // Final update
                albumTask.Description = $"[blue]Albums scanned: {processedAlbums:N0}[/]";
                duplicateTask.Description = $"[yellow]Duplicates found: {duplicateCount:N0}[/]";
                albumTask.StopTask();
                duplicateTask.StopTask();
            });

        var totalElapsed = Stopwatch.GetElapsedTime(scanStartTime);
        Log.Information("Scan complete in {ElapsedSeconds:F2}s: ProcessedArtists={Artists}, ProcessedAlbums={Albums}, UniqueAlbumKeys={Keys}",
            totalElapsed.TotalSeconds, processedArtists, processedAlbums, artistDirectories.Count);

        foreach (var kvp in artistDirectories.Where(x => x.Value.Count > 1))
        {
            var parts = kvp.Key.Split('|');
            var group = new DuplicateAlbumGroup
            {
                ArtistName = parts[0],
                AlbumName = kvp.Value.First().AlbumName,
                Directories = kvp.Value.OrderBy(x => x.Year).ToList()
            };
            duplicateGroups.Add(group);
            
            Log.Debug("Duplicate group created: Artist={Artist}, Album={Album}, DirectoryCount={Count}, Years={Years}",
                group.ArtistName, group.AlbumName, group.Directories.Count,
                string.Join(", ", group.Directories.Select(d => d.Year?.ToString() ?? "?")));
        }

        Log.Information("Found {GroupCount} duplicate album groups", duplicateGroups.Count);
        return duplicateGroups.OrderBy(x => x.ArtistName).ThenBy(x => x.AlbumName).ToList();
    }

    private static AlbumDirectoryInfo? ParseAlbumDirectory(
        string albumDir,
        string artistName,
        ISerializer serializer,
        int mediaFileCount,
        CancellationToken cancellationToken)
    {
        var dirName = Path.GetFileName(albumDir); // Faster than new DirectoryInfo(albumDir).Name
        var albumName = dirName;
        int? year = null;

        Log.Verbose("Parsing album directory: {AlbumDir}", albumDir);

        // Use pre-compiled regex
        var yearMatch = YearRegex.Match(dirName);
        if (yearMatch.Success)
        {
            var yearStr = yearMatch.Groups[1].Success ? yearMatch.Groups[1].Value :
                          yearMatch.Groups[2].Success ? yearMatch.Groups[2].Value :
                          yearMatch.Groups[3].Value;
            if (int.TryParse(yearStr, out var parsedYear) && parsedYear >= 1900 && parsedYear <= DateTime.Now.Year + 1)
            {
                year = parsedYear;
                albumName = YearRemovalRegex.Replace(dirName, " ").Trim();
                Log.Verbose("Parsed year {Year} from directory name, album name: {AlbumName}", year, albumName);
            }
        }

        // Only look for melodee file if we don't have year from directory name (optimization)
        if (!year.HasValue)
        {
            var melodeeFile = Directory.GetFiles(albumDir, $"*{MelodeeModels.Album.JsonFileName}", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();

            if (melodeeFile != null)
            {
                Log.Verbose("Found melodee file: {MelodeeFile}", melodeeFile);
                try
                {
                    var album = MelodeeModels.Album.DeserializeAndInitializeAlbumAsync(serializer, melodeeFile, cancellationToken)
                        .GetAwaiter().GetResult();
                    if (album != null)
                    {
                        albumName = album.AlbumTitle() ?? albumName;
                        year = album.AlbumYear() ?? year;
                        Log.Verbose("Parsed from melodee file - Album: {AlbumName}, Year: {Year}", albumName, year);
                    }
                }
                catch (Exception ex)
                {
                    Log.Verbose(ex, "Failed to parse melodee file: {MelodeeFile}", melodeeFile);
                }
            }
        }

        if (string.IsNullOrWhiteSpace(albumName))
        {
            Log.Verbose("Skipping album directory with empty album name: {AlbumDir}", albumDir);
            return null;
        }

        Log.Verbose("Album parsed: Name={AlbumName}, Year={Year}, FileCount={FileCount}",
            albumName, year, mediaFileCount);

        return new AlbumDirectoryInfo
        {
            Path = albumDir,
            AlbumName = albumName,
            AlbumNameNormalized = albumName.ToNormalizedString() ?? albumName,
            Year = year,
            FileCount = mediaFileCount
        };
    }

    private static void OutputTable(List<DuplicateAlbumGroup> groups, bool hasMetadata)
    {
        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn(new TableColumn("[bold]Artist[/]"));
        table.AddColumn(new TableColumn("[bold]Album[/]"));
        table.AddColumn(new TableColumn("[bold]Directory[/]"));
        table.AddColumn(new TableColumn("[bold]Year[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Files[/]").RightAligned());

        if (hasMetadata)
        {
            table.AddColumn(new TableColumn("[bold]Metadata Year[/]").RightAligned());
            table.AddColumn(new TableColumn("[bold]Source[/]"));
            table.AddColumn(new TableColumn("[bold]Status[/]"));
        }

        foreach (var group in groups)
        {
            var isFirst = true;
            foreach (var dir in group.Directories)
            {
                var statusColor = "grey";
                var statusText = "-";

                if (hasMetadata && group.MetadataYear.HasValue)
                {
                    if (dir.IsCorrectYear == true)
                    {
                        statusColor = "green";
                        statusText = "✓ Keep";
                    }
                    else if (dir.IsCorrectYear == false)
                    {
                        statusColor = "red";
                        statusText = "✗ Delete";
                    }
                    else
                    {
                        statusText = "? Unknown";
                    }
                }

                var yearDisplay = dir.Year?.ToString() ?? "?";
                var yearColor = hasMetadata && group.MetadataYear.HasValue
                    ? (dir.Year == group.MetadataYear ? "green" : "yellow")
                    : "white";

                if (hasMetadata)
                {
                    table.AddRow(
                        isFirst ? group.ArtistName.EscapeMarkup() : string.Empty,
                        isFirst ? group.AlbumName.EscapeMarkup() : string.Empty,
                        Path.GetFileName(dir.Path).EscapeMarkup(),
                        $"[{yearColor}]{yearDisplay}[/]",
                        dir.FileCount.ToString(),
                        isFirst && group.MetadataYear.HasValue ? $"[cyan]{group.MetadataYear}[/]" : string.Empty,
                        isFirst ? (group.MetadataSource ?? "-").EscapeMarkup() : string.Empty,
                        $"[{statusColor}]{statusText}[/]"
                    );
                }
                else
                {
                    table.AddRow(
                        isFirst ? group.ArtistName.EscapeMarkup() : string.Empty,
                        isFirst ? group.AlbumName.EscapeMarkup() : string.Empty,
                        Path.GetFileName(dir.Path).EscapeMarkup(),
                        yearDisplay,
                        dir.FileCount.ToString()
                    );
                }

                isFirst = false;
            }

            table.AddEmptyRow();
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var totalGroups = groups.Count;
        var totalDirs = groups.Sum(g => g.Directories.Count);
        var resolvedGroups = groups.Count(g => g.MetadataYear.HasValue);
        var dirsToDelete = groups.Sum(g => g.SuggestedDeleteDirectories?.Length ?? 0);

        AnsiConsole.MarkupLine($"[grey]Found {totalGroups} duplicate album group(s) with {totalDirs} directories[/]");

        if (hasMetadata)
        {
            AnsiConsole.MarkupLine($"  [cyan]Resolved via metadata:[/] {resolvedGroups}");
            if (dirsToDelete > 0)
            {
                AnsiConsole.MarkupLine($"  [yellow]Directories suggested for deletion:[/] {dirsToDelete}");
            }
        }
    }

    private static void OutputJson(List<DuplicateAlbumGroup> groups, long startTime)
    {
        var elapsed = Stopwatch.GetElapsedTime(startTime);
        var output = new
        {
            durationSeconds = elapsed.TotalSeconds,
            totalGroups = groups.Count,
            totalDirectories = groups.Sum(g => g.Directories.Count),
            resolvedGroups = groups.Count(g => g.MetadataYear.HasValue),
            directoriesToDelete = groups.Sum(g => g.SuggestedDeleteDirectories?.Length ?? 0),
            groups = groups.Select(g => new
            {
                artistName = g.ArtistName,
                albumName = g.AlbumName,
                metadataYear = g.MetadataYear,
                metadataSource = g.MetadataSource,
                musicBrainzId = g.MusicBrainzId,
                spotifyId = g.SpotifyId,
                suggestedCorrectDirectory = g.SuggestedCorrectDirectory,
                suggestedDeleteDirectories = g.SuggestedDeleteDirectories,
                directories = g.Directories.Select(d => new
                {
                    path = d.Path,
                    albumName = d.AlbumName,
                    year = d.Year,
                    fileCount = d.FileCount,
                    isCorrectYear = d.IsCorrectYear
                })
            })
        };

        Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static async Task DeleteDuplicateDirectoriesAsync(
        List<DuplicateAlbumGroup> groups,
        CancellationToken cancellationToken)
    {
        var dirsToDelete = groups
            .Where(g => g.SuggestedDeleteDirectories?.Length > 0)
            .SelectMany(g => g.SuggestedDeleteDirectories!)
            .ToArray();

        Log.Debug("DeleteDuplicateDirectoriesAsync: {DirCount} directories identified for deletion", dirsToDelete.Length);
        
        if (dirsToDelete.Length == 0)
        {
            Log.Information("No directories identified for deletion");
            AnsiConsole.MarkupLine("[yellow]No directories identified for deletion.[/]");
            return;
        }

        Log.Debug("Directories to delete: {Directories}", string.Join(", ", dirsToDelete.Select(Path.GetFileName)));

        var confirmed = AnsiConsole.Confirm(
            $"[yellow]Are you sure you want to delete {dirsToDelete.Length} duplicate directories?[/]",
            defaultValue: false);

        if (!confirmed)
        {
            Log.Information("Delete operation cancelled by user");
            AnsiConsole.MarkupLine("[grey]Delete operation cancelled.[/]");
            return;
        }

        Log.Information("User confirmed deletion of {DirCount} directories", dirsToDelete.Length);
        
        var successCount = 0;
        var failCount = 0;
        var deleteStartTime = Stopwatch.GetTimestamp();

        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[red]Deleting duplicate directories...[/]", maxValue: dirsToDelete.Length);

                foreach (var dir in dirsToDelete)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Log.Warning("Delete operation cancelled by user");
                        break;
                    }

                    try
                    {
                        if (Directory.Exists(dir))
                        {
                            Log.Debug("Deleting directory: {Directory}", dir);
                            Directory.Delete(dir, true);
                            successCount++;
                            Log.Information("Successfully deleted directory: {Directory}", dir);
                            AnsiConsole.MarkupLine($"  [green]✓[/] Deleted: {Path.GetFileName(dir).EscapeMarkup()}");
                        }
                        else
                        {
                            Log.Warning("Directory no longer exists: {Directory}", dir);
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        Log.Error(ex, "Failed to delete directory: {Directory}", dir);
                        AnsiConsole.MarkupLine($"  [red]✗[/] Failed to delete {Path.GetFileName(dir).EscapeMarkup()}: {ex.Message.EscapeMarkup()}");
                    }

                    task.Increment(1);
                    await Task.Yield();
                }
            });

        var deleteElapsed = Stopwatch.GetElapsedTime(deleteStartTime);
        Log.Information("Delete operation completed in {ElapsedSeconds:F2}s. Succeeded={Success}, Failed={Failed}",
            deleteElapsed.TotalSeconds, successCount, failCount);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Delete complete:[/] [green]{successCount} succeeded[/], [red]{failCount} failed[/]");
    }

    private sealed class DuplicateAlbumGroup
    {
        public required string ArtistName { get; init; }
        public required string AlbumName { get; init; }
        public List<AlbumDirectoryInfo> Directories { get; init; } = [];
        public int? MetadataYear { get; set; }
        public string? MetadataSource { get; set; }
        public Guid? MusicBrainzId { get; set; }
        public string? SpotifyId { get; set; }
        public string? SuggestedCorrectDirectory { get; set; }
        public string[]? SuggestedDeleteDirectories { get; set; }
    }

    private sealed class AlbumDirectoryInfo
    {
        public required string Path { get; init; }
        public required string AlbumName { get; init; }
        public required string AlbumNameNormalized { get; init; }
        public int? Year { get; init; }
        public int FileCount { get; init; }
        public bool? IsCorrectYear { get; set; }
    }
}
