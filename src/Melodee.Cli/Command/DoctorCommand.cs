using System.Diagnostics;
using System.Text.Json;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

public sealed class DoctorCommand : CommandBase<DoctorSettings>
{
    private sealed record CheckResult(string Name, bool Success, string Details, TimeSpan Duration);

    private sealed record LibraryPathResult(
        string Name,
        string Type,
        string Path,
        bool Exists,
        bool Writable,
        string Details);

    public override async Task<int> ExecuteAsync(CommandContext context, DoctorSettings settings, CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.StartNew();

        var checks = new List<CheckResult>();
        var libraries = new List<LibraryPathResult>();

        var configPathInfo = GetConfigurationPathInfo();

        IConfigurationRoot config;
        CheckResult configCheck;
        try
        {
            config = Configuration();

            var sw = Stopwatch.StartNew();
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var details = $"Environment={env}; {configPathInfo}";

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(config.GetConnectionString("DefaultConnection")))
            {
                missing.Add("ConnectionStrings:DefaultConnection");
            }
            if (string.IsNullOrWhiteSpace(config.GetConnectionString("MusicBrainzConnection")))
            {
                missing.Add("ConnectionStrings:MusicBrainzConnection");
            }
            if (string.IsNullOrWhiteSpace(config.GetConnectionString("ArtistSearchEngineConnection")))
            {
                missing.Add("ConnectionStrings:ArtistSearchEngineConnection");
            }

            configCheck = missing.Count != 0
                ? new CheckResult("Configuration", false, $"Missing: {string.Join(", ", missing)}; {details}", sw.Elapsed)
                : new CheckResult("Configuration", true, details, sw.Elapsed);
        }
        catch (Exception ex)
        {
            configCheck = new CheckResult("Configuration", false, ex.Message, TimeSpan.Zero);
            checks.Add(configCheck);

            if (settings.ReturnRaw)
            {
                var obj = new
                {
                    success = false,
                    durationSeconds = startedAt.Elapsed.TotalSeconds,
                    configuration = configPathInfo,
                    checks = checks.Select(c => new { name = c.Name, success = c.Success, details = c.Details, durationMs = (int)c.Duration.TotalMilliseconds }),
                    libraries
                };

                Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
                return 1;
            }

            RenderSummary(checks, libraries, startedAt.Elapsed, settings.WriteTest);
            return 1;
        }

        if (!configCheck.Success)
        {
            checks.Add(configCheck);

            if (settings.ReturnRaw)
            {
                var obj = new
                {
                    success = false,
                    durationSeconds = startedAt.Elapsed.TotalSeconds,
                    configuration = configPathInfo,
                    checks = checks.Select(c => new { name = c.Name, success = c.Success, details = c.Details, durationMs = (int)c.Duration.TotalMilliseconds }),
                    libraries
                };

                Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
                return 1;
            }

            RenderSummary(checks, libraries, startedAt.Elapsed, settings.WriteTest);
            return 1;
        }

        ServiceProvider provider;
        try
        {
            provider = CreateServiceProvider();
        }
        catch (Exception ex)
        {
            checks.Add(configCheck);
            checks.Add(new CheckResult("Service Provider", false, ex.Message, TimeSpan.Zero));

            if (settings.ReturnRaw)
            {
                var obj = new
                {
                    success = false,
                    durationSeconds = startedAt.Elapsed.TotalSeconds,
                    configuration = configPathInfo,
                    checks = checks.Select(c => new { name = c.Name, success = c.Success, details = c.Details, durationMs = (int)c.Duration.TotalMilliseconds }),
                    libraries
                };

                Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
                return 1;
            }

            RenderSummary(checks, libraries, startedAt.Elapsed, settings.WriteTest);
            return 1;
        }

        using var scope = provider.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>();
        var mbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MusicBrainzDbContext>>();
        var aseFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ArtistSearchEngineServiceDbContext>>();
        var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();

        await AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn()
            ])
            .StartAsync(async progress =>
            {
                await RunCheckAsync(progress, checks, "Configuration", () => Task.FromResult(configCheck));

                await RunCheckAsync(progress, checks, "Database: Postgres", async () =>
                {
                    var sw = Stopwatch.StartNew();
                    await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

                    var ok = await db.Database.CanConnectAsync(cancellationToken);
                    var details = ok
                        ? $"OK ({db.Database.ProviderName})"
                        : "Unable to connect";

                    return new CheckResult("Database: Postgres", ok, details, sw.Elapsed);
                });

                await RunCheckAsync(progress, checks, "Database: MusicBrainz (SQLite)", async () =>
                {
                    var sw = Stopwatch.StartNew();

                    var cs = config.GetConnectionString("MusicBrainzConnection") ?? string.Empty;
                    var fileInfo = DescribeSqlitePath(cs);

                    await using var db = await mbFactory.CreateDbContextAsync(cancellationToken);
                    var ok = await db.Database.CanConnectAsync(cancellationToken);

                    return new CheckResult(
                        "Database: MusicBrainz (SQLite)",
                        ok,
                        ok ? $"OK; {fileInfo}" : $"Unable to connect; {fileInfo}",
                        sw.Elapsed);
                });

                await RunCheckAsync(progress, checks, "Database: ArtistSearchEngine (SQLite)", async () =>
                {
                    var sw = Stopwatch.StartNew();

                    var cs = config.GetConnectionString("ArtistSearchEngineConnection") ?? string.Empty;
                    var fileInfo = DescribeSqlitePath(cs);

                    await using var db = await aseFactory.CreateDbContextAsync(cancellationToken);
                    var ok = await db.Database.CanConnectAsync(cancellationToken);

                    return new CheckResult(
                        "Database: ArtistSearchEngine (SQLite)",
                        ok,
                        ok ? $"OK; {fileInfo}" : $"Unable to connect; {fileInfo}",
                        sw.Elapsed);
                });

                await RunCheckAsync(progress, checks, "Libraries", async () =>
                {
                    var sw = Stopwatch.StartNew();

                    var libs = await libraryService.ListAsync(new PagedRequest { PageSize = short.MaxValue }, cancellationToken);
                    if (!libs.IsSuccess)
                    {
                        return new CheckResult("Libraries", false, libs.Messages?.FirstOrDefault() ?? "Failed to list libraries", sw.Elapsed);
                    }

                    foreach (var lib in libs.Data)
                    {
                        var exists = Directory.Exists(lib.Path);
                        var writable = false;
                        var details = exists ? "Path exists" : "Path missing";

                        if (exists && settings.WriteTest)
                        {
                            try
                            {
                                var testFile = Path.Combine(lib.Path, $".mcli-doctor-{Guid.NewGuid():N}.tmp");
                                await File.WriteAllTextAsync(testFile, string.Empty, cancellationToken);
                                File.Delete(testFile);
                                writable = true;
                                details = "Path exists; write OK";
                            }
                            catch (Exception ex)
                            {
                                writable = false;
                                details = $"Path exists; write failed: {ex.GetType().Name}";
                            }
                        }

                        libraries.Add(new LibraryPathResult(
                            lib.Name,
                            lib.TypeValue.ToString(),
                            lib.Path,
                            exists,
                            settings.WriteTest ? writable : false,
                            details));
                    }

                    var anyMissing = libraries.Any(l => !l.Exists);
                    return new CheckResult(
                        "Libraries",
                        !anyMissing,
                        anyMissing
                            ? "One or more library paths are missing"
                            : (settings.WriteTest ? "All library paths exist (write test enabled)" : "All library paths exist"),
                        sw.Elapsed);
                });
            });

        if (settings.ReturnRaw)
        {
            var obj = new
            {
                success = checks.All(c => c.Success),
                durationSeconds = startedAt.Elapsed.TotalSeconds,
                configuration = configPathInfo,
                checks = checks.Select(c => new { name = c.Name, success = c.Success, details = c.Details, durationMs = (int)c.Duration.TotalMilliseconds }),
                libraries
            };

            Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
            return checks.All(c => c.Success) ? 0 : 1;
        }

        RenderSummary(checks, libraries, startedAt.Elapsed, settings.WriteTest);

        return checks.All(c => c.Success) ? 0 : 1;
    }

    private static async Task RunCheckAsync(ProgressContext progress, List<CheckResult> results, string name, Func<Task<CheckResult>> action)
    {
        var task = progress.AddTask($"{name}...", maxValue: 1);
        var result = await action();
        task.Increment(1);

        var icon = result.Success ? "[green]✓[/]" : "[red]✗[/]";
        task.Description = $"{icon} {name}";

        results.Add(result);
    }

    private static void RenderSummary(IReadOnlyCollection<CheckResult> checks, IReadOnlyCollection<LibraryPathResult> libraries, TimeSpan elapsed, bool writeTest)
    {
        var header = new Panel(new Markup($"[bold cyan]mcli doctor[/] completed in [grey]{elapsed:c}[/]"))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Grey)
        };
        AnsiConsole.Write(header);
        AnsiConsole.WriteLine();

        var table = new Table().RoundedBorder();
        table.AddColumn("Check");
        table.AddColumn("Status");
        table.AddColumn("Details");
        table.AddColumn(new TableColumn("Duration").RightAligned());

        foreach (var c in checks)
        {
            table.AddRow(
                c.Name.EscapeMarkup(),
                c.Success ? "[green]OK[/]" : "[red]FAIL[/]",
                c.Details.EscapeMarkup(),
                $"{c.Duration.TotalMilliseconds:0}ms");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        if (libraries.Count != 0)
        {
            var libTable = new Table().RoundedBorder();
            libTable.AddColumn("Library");
            libTable.AddColumn("Type");
            libTable.AddColumn("Exists");
            if (writeTest)
            {
                libTable.AddColumn("Writable");
            }
            libTable.AddColumn("Path");
            libTable.AddColumn("Details");

            foreach (var l in libraries.OrderBy(l => l.Type).ThenBy(l => l.Name))
            {
                var existsText = l.Exists ? "[green]✓[/]" : "[red]✗[/]";
                var writableText = l.Writable ? "[green]✓[/]" : "[red]✗[/]";

                if (writeTest)
                {
                    libTable.AddRow(
                        l.Name.EscapeMarkup(),
                        l.Type.EscapeMarkup(),
                        existsText,
                        writableText,
                        $"[dim]{l.Path.EscapeMarkup()}[/]",
                        l.Details.EscapeMarkup());
                }
                else
                {
                    libTable.AddRow(
                        l.Name.EscapeMarkup(),
                        l.Type.EscapeMarkup(),
                        existsText,
                        $"[dim]{l.Path.EscapeMarkup()}[/]",
                        l.Details.EscapeMarkup());
                }
            }

            AnsiConsole.Write(new Panel(libTable)
            {
                Header = new PanelHeader("[bold]Library Paths[/]", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(foreground: Color.Grey)
            });
            AnsiConsole.WriteLine();
        }

        var failed = checks.Where(c => !c.Success).Select(c => c.Name).ToList();
        if (failed.Count != 0)
        {
            AnsiConsole.MarkupLine($"[red]Doctor found issues:[/] {string.Join(", ", failed.Select(x => x.EscapeMarkup()))}");
            AnsiConsole.MarkupLine("[grey]Tip:[/] verify MELODEE_APPSETTINGS_PATH, connection strings, and library path mounts/permissions.");
        }
        else
        {
            AnsiConsole.MarkupLine("[green]All checks passed.[/]");
        }
    }

    private static string DescribeSqlitePath(string connectionString)
    {
        try
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(builder.DataSource))
            {
                return "DataSource=(empty)";
            }

            var fullPath = builder.DataSource;
            var exists = File.Exists(fullPath);
            return $"DataSource={fullPath}; Exists={exists}";
        }
        catch
        {
            return "DataSource=(unparseable)";
        }
    }

    private static string GetConfigurationPathInfo()
    {
        var appSettingsPath = Environment.GetEnvironmentVariable("MELODEE_APPSETTINGS_PATH");
        if (!string.IsNullOrWhiteSpace(appSettingsPath))
        {
            return File.Exists(appSettingsPath)
                ? $"MELODEE_APPSETTINGS_PATH={appSettingsPath}"
                : $"MELODEE_APPSETTINGS_PATH={appSettingsPath} (missing)";
        }

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var basePath = Directory.GetCurrentDirectory();
        var defaultFile = Path.Combine(basePath, "appsettings.json");
        var envFile = Path.Combine(basePath, $"appsettings.{env}.json");

        var defaultExists = File.Exists(defaultFile);
        var envExists = File.Exists(envFile);

        return $"appsettings.json={defaultExists}; appsettings.{env}.json={envExists}; cwd={basePath}";
    }
}
