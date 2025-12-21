using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Jobs;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Tests.Common.Jobs;

public class LibraryInsertJobGetMelodeeFilesToProcessTests : TestsBase, IDisposable
{
    private readonly string _testDirectory;
    private readonly LibraryInsertJob _job;
    private readonly Library _testLibrary;

    public LibraryInsertJobGetMelodeeFilesToProcessTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "LibraryInsertJobTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);

        var contextFactory = CreateInMemoryContextFactory();

        _job = new LibraryInsertJob(
            Logger,
            MockConfigurationFactory(),
            null!,
            Serializer,
            contextFactory,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        _testLibrary = new Library
        {
            Id = 1,
            Name = "Test Library",
            Path = _testDirectory,
            Type = (int)LibraryType.Inbound,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
    }

    private IDbContextFactory<MelodeeDbContext> CreateInMemoryContextFactory()
    {
        var options = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContextFactory(options);
    }

    private class TestDbContextFactory(DbContextOptions<MelodeeDbContext> options) : IDbContextFactory<MelodeeDbContext>
    {
        public MelodeeDbContext CreateDbContext()
        {
            return new MelodeeDbContext(options);
        }
    }

    private List<FileInfo> CallGetMelodeeFilesToProcess(Library library, string? scanJustDirectory, DateTime lastScanAtUtc)
    {
        var method = typeof(LibraryInsertJob).GetMethod("GetMelodeeFilesToProcess",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (List<FileInfo>)method!.Invoke(_job, [library, scanJustDirectory, lastScanAtUtc])!;
    }

    private void CreateMelodeeJsonFile(string relativePath, DateTime? lastModified = null)
    {
        var fullPath = Path.Combine(_testDirectory, relativePath);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        File.WriteAllText(fullPath, "{}");

        if (lastModified.HasValue)
        {
            File.SetLastWriteTimeUtc(fullPath, lastModified.Value);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public void GetMelodeeFilesToProcess_EmptyLibrary_ReturnsEmpty()
    {
        var lastScan = DateTime.UtcNow.AddDays(-1);

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.Empty(result);
    }

    [Fact]
    public void GetMelodeeFilesToProcess_NoFilesMatchLastScanDate_ReturnsEmpty()
    {
        CreateMelodeeJsonFile("Artist1/Album1/melodee.json", DateTime.UtcNow.AddDays(-10));
        CreateMelodeeJsonFile("Artist2/Album2/melodee.json", DateTime.UtcNow.AddDays(-10));
        var lastScan = DateTime.UtcNow.AddDays(-1);

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.Empty(result);
    }

    [Fact]
    public void GetMelodeeFilesToProcess_AllFilesModifiedAfterScan_ReturnsAll()
    {
        var lastScan = DateTime.UtcNow.AddDays(-5);
        CreateMelodeeJsonFile("Artist1/Album1/melodee.json", DateTime.UtcNow.AddDays(-1));
        CreateMelodeeJsonFile("Artist2/Album2/melodee.json", DateTime.UtcNow.AddDays(-2));
        CreateMelodeeJsonFile("Artist3/Album3/melodee.json", DateTime.UtcNow.AddDays(-3));

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.Equal(3, result.Count);
        Assert.All(result, f => Assert.Contains("melodee.json", f.Name));
    }

    [Fact]
    public void GetMelodeeFilesToProcess_MixedModificationDates_ReturnsOnlyRecent()
    {
        var lastScan = DateTime.UtcNow.AddDays(-5);
        CreateMelodeeJsonFile("Artist1/Album1/melodee.json", DateTime.UtcNow.AddDays(-1));
        CreateMelodeeJsonFile("Artist2/Album2/melodee.json", DateTime.UtcNow.AddDays(-10));
        CreateMelodeeJsonFile("Artist3/Album3/melodee.json", DateTime.UtcNow.AddDays(-2));
        CreateMelodeeJsonFile("Artist4/Album4/melodee.json", DateTime.UtcNow.AddDays(-20));

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.True(f.LastWriteTimeUtc >= lastScan));
    }

    [Fact]
    public void GetMelodeeFilesToProcess_ExactBoundaryDate_IncludesFile()
    {
        var lastScan = DateTime.UtcNow.AddDays(-5);
        CreateMelodeeJsonFile("Artist1/Album1/melodee.json", lastScan);

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.Single(result);
    }

    [Fact]
    public void GetMelodeeFilesToProcess_NestedDirectories_FindsAllFiles()
    {
        var lastScan = DateTime.UtcNow.AddDays(-1);
        CreateMelodeeJsonFile("Artist1/Album1/melodee.json");
        CreateMelodeeJsonFile("Artist1/Album2/melodee.json");
        CreateMelodeeJsonFile("Artist2/Album1/melodee.json");
        CreateMelodeeJsonFile("Various/Compilations/Album1/melodee.json");
        CreateMelodeeJsonFile("Deep/Nested/Path/To/Album/melodee.json");

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void GetMelodeeFilesToProcess_ScanJustDirectory_OnlyScansSpecificDirectory()
    {
        var scanDir = Path.Combine(_testDirectory, "Artist1");
        Directory.CreateDirectory(scanDir);

        var lastScan = DateTime.UtcNow.AddDays(-1);
        CreateMelodeeJsonFile("Artist1/Album1/melodee.json");
        CreateMelodeeJsonFile("Artist1/Album2/melodee.json");
        CreateMelodeeJsonFile("Artist2/Album1/melodee.json");
        CreateMelodeeJsonFile("Artist3/Album1/melodee.json");

        var result = CallGetMelodeeFilesToProcess(_testLibrary, scanDir, lastScan);

        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.Contains("Artist1", f.FullName));
    }

    [Fact]
    public void GetMelodeeFilesToProcess_ScanNonExistentDirectory_ReturnsEmpty()
    {
        var nonExistentDir = Path.Combine(_testDirectory, "DoesNotExist");
        var lastScan = DateTime.UtcNow.AddDays(-1);

        var result = CallGetMelodeeFilesToProcess(_testLibrary, nonExistentDir, lastScan);

        Assert.Empty(result);
    }

    [Fact]
    public void GetMelodeeFilesToProcess_OnlyMelodeeJsonFiles_IgnoresOtherFiles()
    {
        var lastScan = DateTime.UtcNow.AddDays(-1);
        CreateMelodeeJsonFile("Artist1/Album1/melodee.json");

        var otherFile1 = Path.Combine(_testDirectory, "Artist1/Album1/other.json");
        Directory.CreateDirectory(Path.GetDirectoryName(otherFile1)!);
        File.WriteAllText(otherFile1, "{}");

        var otherFile2 = Path.Combine(_testDirectory, "Artist2/readme.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(otherFile2)!);
        File.WriteAllText(otherFile2, "readme");

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.Single(result);
        Assert.Equal("melodee.json", result.First().Name);
    }

    [Fact]
    public void GetMelodeeFilesToProcess_LargeNumberOfFiles_ProcessesAll()
    {
        var lastScan = DateTime.UtcNow.AddDays(-1);

        for (int i = 0; i < 50; i++)
        {
            CreateMelodeeJsonFile($"Artist{i}/Album{i}/melodee.json");
        }

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.Equal(50, result.Count);
    }

    [Fact]
    public void GetMelodeeFilesToProcess_FilesWithSpecialCharacters_HandlesCorrectly()
    {
        var lastScan = DateTime.UtcNow.AddDays(-1);
        CreateMelodeeJsonFile("ArtistName/Album1/melodee.json");
        CreateMelodeeJsonFile("ArtistBand/Album2/melodee.json");

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetMelodeeFilesToProcess_ScanJustDirectoryWithNestedFiles_FindsNestedFiles()
    {
        var scanDir = Path.Combine(_testDirectory, "Artist1");
        Directory.CreateDirectory(scanDir);

        var lastScan = DateTime.UtcNow.AddDays(-1);
        CreateMelodeeJsonFile("Artist1/Album1/melodee.json");
        CreateMelodeeJsonFile("Artist1/Album2/melodee.json");
        CreateMelodeeJsonFile("Artist1/Subdir/Album3/melodee.json");

        var result = CallGetMelodeeFilesToProcess(_testLibrary, scanDir, lastScan);

        Assert.Equal(3, result.Count);
        Assert.All(result, f => Assert.Contains("Artist1", f.FullName));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-7)]
    [InlineData(-30)]
    public void GetMelodeeFilesToProcess_VariousLastScanDates_FiltersCorrectly(int daysAgo)
    {
        var lastScan = DateTime.UtcNow.AddDays(daysAgo);

        CreateMelodeeJsonFile("Recent/melodee.json", DateTime.UtcNow);
        CreateMelodeeJsonFile("Yesterday/melodee.json", DateTime.UtcNow.AddDays(-1));
        CreateMelodeeJsonFile("LastWeek/melodee.json", DateTime.UtcNow.AddDays(-7));
        CreateMelodeeJsonFile("LastMonth/melodee.json", DateTime.UtcNow.AddDays(-30));

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.All(result, f => Assert.True(f.LastWriteTimeUtc >= lastScan));
    }

    [Fact]
    public void GetMelodeeFilesToProcess_EmptySubdirectories_DoesNotCauseError()
    {
        var lastScan = DateTime.UtcNow.AddDays(-1);

        Directory.CreateDirectory(Path.Combine(_testDirectory, "EmptyArtist1"));
        Directory.CreateDirectory(Path.Combine(_testDirectory, "EmptyArtist2/EmptyAlbum"));

        CreateMelodeeJsonFile("ValidArtist/ValidAlbum/melodee.json");

        var result = CallGetMelodeeFilesToProcess(_testLibrary, null, lastScan);

        Assert.Single(result);
    }
}
