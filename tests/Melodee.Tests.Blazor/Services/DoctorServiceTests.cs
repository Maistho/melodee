using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Melodee.Tests.Blazor.Services;

/// <summary>
/// Tests for DoctorService.
/// Verifies health check and diagnostic functionality.
/// </summary>
public class DoctorServiceTests
{
    private readonly Mock<IDbContextFactory<MelodeeDbContext>> _dbContextFactory;
    private readonly Mock<IDbContextFactory<MusicBrainzDbContext>> _musicBrainzDbContextFactory;
    private readonly Mock<IDbContextFactory<ArtistSearchEngineServiceDbContext>> _artistSearchEngineDbContextFactory;
    private readonly Mock<LibraryService> _libraryService;
    private readonly Mock<IWebHostEnvironment> _webHostEnvironment;

    public DoctorServiceTests()
    {
        _dbContextFactory = new Mock<IDbContextFactory<MelodeeDbContext>>();
        _musicBrainzDbContextFactory = new Mock<IDbContextFactory<MusicBrainzDbContext>>();
        _artistSearchEngineDbContextFactory = new Mock<IDbContextFactory<ArtistSearchEngineServiceDbContext>>();
        _libraryService = new Mock<LibraryService>();
        _webHostEnvironment = new Mock<IWebHostEnvironment>();
        _webHostEnvironment.Setup(x => x.EnvironmentName).Returns("Test");
        _webHostEnvironment.Setup(x => x.ContentRootPath).Returns("/test/path");
    }

    [Fact]
    public async Task NeedsAttentionAsync_MissingConnectionStrings_ReturnsTrue()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var service = CreateService(configuration);

        var result = await service.NeedsAttentionAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task IsMusicBrainzDatabaseEmptyAsync_NoConnectionString_ReturnsTrue()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var service = CreateService(configuration);

        var result = await service.IsMusicBrainzDatabaseEmptyAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task IsMusicBrainzDatabaseEmptyAsync_EmptyConnectionString_ReturnsTrue()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:MusicBrainzConnection"] = ""
        });
        var service = CreateService(configuration);

        var result = await service.IsMusicBrainzDatabaseEmptyAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task IsMusicBrainzDatabaseEmptyAsync_FileDoesNotExist_ReturnsTrue()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.db");
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:MusicBrainzConnection"] = $"Data Source={tempPath}"
        });
        var service = CreateService(configuration);

        var result = await service.IsMusicBrainzDatabaseEmptyAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task IsMusicBrainzDatabaseEmptyAsync_EmptyFile_ReturnsTrue()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"empty-{Guid.NewGuid()}.db");
        try
        {
            File.WriteAllText(tempPath, "");
            var configuration = CreateConfiguration(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MusicBrainzConnection"] = $"Data Source={tempPath}"
            });
            var service = CreateService(configuration);

            var result = await service.IsMusicBrainzDatabaseEmptyAsync();

            Assert.True(result);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public async Task RunAllChecksAsync_ReturnsCheckResults()
    {
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        Assert.NotNull(results);
        Assert.NotNull(results.Checks);
        Assert.True(results.Checks.Count > 0);
    }

    [Fact]
    public async Task RunAllChecksAsync_IncludesConfigurationCheck()
    {
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        var configCheck = results.Checks.FirstOrDefault(c => c.Name == "Configuration");
        Assert.NotNull(configCheck);
    }

    [Fact]
    public async Task RunAllChecksAsync_MissingConfig_ConfigCheckFails()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        var configCheck = results.Checks.FirstOrDefault(c => c.Name == "Configuration");
        Assert.NotNull(configCheck);
        Assert.False(configCheck.Success);
        Assert.Contains("Missing", configCheck.Details);
    }

    [Fact]
    public async Task RunAllChecksAsync_HasIssues_WhenChecksFail()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>());
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        Assert.True(results.HasIssues);
    }

    [Fact]
    public async Task RunAllChecksAsync_CheckResultsHaveDuration()
    {
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        foreach (var check in results.Checks)
        {
            Assert.True(check.Duration >= TimeSpan.Zero, $"Check '{check.Name}' should have non-negative duration");
        }
    }

    [Fact]
    public async Task RunAllChecksAsync_ReturnsConnectionStringInfo()
    {
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        Assert.NotNull(results.ConnectionStrings);
        Assert.True(results.ConnectionStrings.Count > 0);
    }

    [Fact]
    public async Task RunAllChecksAsync_ReturnsEnvironmentVariableInfo()
    {
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        Assert.NotNull(results.EnvironmentVariables);
    }

    [Fact]
    public async Task RunAllChecksAsync_ReturnsDiskSpaceInfo()
    {
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        Assert.NotNull(results.DiskSpaceInfo);
    }

    [Fact]
    public async Task RunAllChecksAsync_ReturnsSearchEngineApiKeysInfo()
    {
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        Assert.NotNull(results.SearchEngineApiKeys);
    }

    [Fact]
    public async Task RunAllChecksAsync_IncludesDiskSpaceCheck()
    {
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        var diskSpaceCheck = results.Checks.FirstOrDefault(c => c.Name == "DiskSpace");
        Assert.NotNull(diskSpaceCheck);
    }

    [Fact]
    public async Task RunAllChecksAsync_IncludesLibraryPathOverlapCheck()
    {
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        var overlapCheck = results.Checks.FirstOrDefault(c => c.Name == "LibraryPathOverlap");
        Assert.NotNull(overlapCheck);
    }

    [Fact]
    public async Task RunAllChecksAsync_IncludesSearchEngineApiKeysCheck()
    {
        var configuration = CreateValidConfiguration();
        var service = CreateService(configuration);

        var results = await service.RunAllChecksAsync();

        var apiKeysCheck = results.Checks.FirstOrDefault(c => c.Name == "SearchEngineApiKeys");
        Assert.NotNull(apiKeysCheck);
    }

    private DoctorService CreateService(IConfiguration configuration)
    {
        return new DoctorService(
            configuration,
            _dbContextFactory.Object,
            _musicBrainzDbContextFactory.Object,
            _artistSearchEngineDbContextFactory.Object,
            _libraryService.Object,
            _webHostEnvironment.Object);
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static IConfiguration CreateValidConfiguration()
    {
        return CreateConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test",
            ["ConnectionStrings:MusicBrainzConnection"] = "Data Source=/tmp/test.db",
            ["ConnectionStrings:ArtistSearchEngineConnection"] = "Data Source=/tmp/test2.db"
        });
    }
}
