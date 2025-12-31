using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Services;

/// <summary>
/// Tests for album file merge operations.
/// These tests verify that files are correctly merged between source and target directories,
/// including proper handling of duplicate files (by name and by content size).
/// </summary>
public sealed class AlbumFileMergeTests : IDisposable
{
    private readonly string _testDir;

    public AlbumFileMergeTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"melodee_merge_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    #region Duplicate File Handling Tests

    [Fact]
    public async Task MergeFiles_SameFileName_DeletesSourceFile()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        CreateFile(sourceDir, "0001 Track One.mp3", 1000);
        CreateFile(targetDir, "0001 Track One.mp3", 1000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.Equal(0, result.FilesMoved);
        Assert.Equal(1, result.FilesDeleted);
        Assert.False(File.Exists(Path.Combine(sourceDir, "0001 Track One.mp3")));
        Assert.True(File.Exists(Path.Combine(targetDir, "0001 Track One.mp3")));
    }

    [Fact]
    public async Task MergeFiles_SameFileSize_DeletesSourceFile()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        CreateFile(sourceDir, "0001 Different Name.mp3", 1234);
        CreateFile(targetDir, "0001 Track One.mp3", 1234);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.Equal(0, result.FilesMoved);
        Assert.Equal(1, result.FilesDeleted);
        Assert.False(File.Exists(Path.Combine(sourceDir, "0001 Different Name.mp3")));
    }

    [Fact]
    public async Task MergeFiles_UniqueFile_MovesToTarget()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        CreateFile(sourceDir, "0001 Unique Track.mp3", 1000);
        CreateFile(targetDir, "0001 Existing Track.mp3", 2000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.Equal(1, result.FilesMoved);
        Assert.Equal(0, result.FilesDeleted);
        Assert.True(File.Exists(Path.Combine(targetDir, "0001 Unique Track.mp3")));
        Assert.False(File.Exists(Path.Combine(sourceDir, "0001 Unique Track.mp3")));
    }

    [Fact]
    public async Task MergeFiles_MixedDuplicatesAndUnique_CorrectlyHandlesBoth()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        // Duplicate by name
        CreateFile(sourceDir, "0001 Track One.mp3", 1000);
        CreateFile(targetDir, "0001 Track One.mp3", 1000);

        // Duplicate by size
        CreateFile(sourceDir, "0002 Track Two Alt.mp3", 2000);
        CreateFile(targetDir, "0002 Track Two.mp3", 2000);

        // Unique file (different name and size)
        CreateFile(sourceDir, "0003 Bonus Track.mp3", 3000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.Equal(1, result.FilesMoved); // Only the bonus track
        Assert.Equal(2, result.FilesDeleted); // Two duplicates
        Assert.True(File.Exists(Path.Combine(targetDir, "0003 Bonus Track.mp3")));
    }

    [Fact]
    public async Task MergeFiles_AllDuplicates_SourceDirectoryBecomesEmpty()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        CreateFile(sourceDir, "0001 Track One.mp3", 1000);
        CreateFile(sourceDir, "0002 Track Two.mp3", 2000);
        CreateFile(sourceDir, "0003 Track Three.mp3", 3000);

        CreateFile(targetDir, "0001 Track One.mp3", 1000);
        CreateFile(targetDir, "0002 Track Two.mp3", 2000);
        CreateFile(targetDir, "0003 Track Three.mp3", 3000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.Equal(0, result.FilesMoved);
        Assert.Equal(3, result.FilesDeleted);

        var remainingMediaFiles = Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories)
            .Count(f => FileHelper.IsFileMediaType(Path.GetExtension(f)));
        Assert.Equal(0, remainingMediaFiles);
    }

    #endregion

    #region Non-Media File Handling Tests

    [Fact]
    public async Task MergeFiles_ImageFiles_MergedToTarget()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        CreateFile(sourceDir, "cover.jpg", 5000);
        CreateFile(sourceDir, "back.jpg", 4000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.True(File.Exists(Path.Combine(targetDir, "cover.jpg")));
        Assert.True(File.Exists(Path.Combine(targetDir, "back.jpg")));
    }

    [Fact]
    public async Task MergeFiles_DuplicateImages_DeletedFromSource()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        CreateFile(sourceDir, "cover.jpg", 5000);
        CreateFile(targetDir, "cover.jpg", 5000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.False(File.Exists(Path.Combine(sourceDir, "cover.jpg")));
        Assert.True(File.Exists(Path.Combine(targetDir, "cover.jpg")));
    }

    [Fact]
    public async Task MergeFiles_MetadataFiles_MergedToTarget()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        CreateFile(sourceDir, "album.nfo", 500);
        CreateFile(sourceDir, "album.cue", 1000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.True(File.Exists(Path.Combine(targetDir, "album.nfo")));
        Assert.True(File.Exists(Path.Combine(targetDir, "album.cue")));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task MergeFiles_EmptySourceDirectory_ReturnsZero()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        CreateFile(targetDir, "0001 Track One.mp3", 1000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.Equal(0, result.FilesMoved);
        Assert.Equal(0, result.FilesDeleted);
    }

    [Fact]
    public async Task MergeFiles_EmptyTargetDirectory_MovesAllFiles()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        CreateFile(sourceDir, "0001 Track One.mp3", 1000);
        CreateFile(sourceDir, "0002 Track Two.mp3", 2000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.Equal(2, result.FilesMoved);
        Assert.Equal(0, result.FilesDeleted);
    }

    [Fact]
    public async Task MergeFiles_TargetDirectoryDoesNotExist_ReturnsZero()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = Path.Combine(_testDir, "nonexistent");

        CreateFile(sourceDir, "0001 Track One.mp3", 1000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.Equal(0, result.FilesMoved);
        Assert.Equal(0, result.FilesDeleted);
    }

    [Fact]
    public async Task MergeFiles_CaseInsensitiveFileNameMatch_TreatedAsDuplicate()
    {
        var sourceDir = CreateDirectory("source");
        var targetDir = CreateDirectory("target");

        CreateFile(sourceDir, "0001 Track One.MP3", 1000);
        CreateFile(targetDir, "0001 track one.mp3", 1000);

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.Equal(0, result.FilesMoved);
        Assert.Equal(1, result.FilesDeleted);
    }

    [Fact]
    public async Task MergeFiles_ReissueWithBonusTracks_BonusTracksMovedToDuplicatesDeleted()
    {
        var sourceDir = CreateDirectory("source_reissue_2010");
        var targetDir = CreateDirectory("target_original_1985");

        // Original 1985 release - 10 tracks
        for (var i = 1; i <= 10; i++)
        {
            CreateFile(targetDir, $"00{i:D2} Track {i}.mp3", i * 1000);
        }

        // 2010 reissue - same 10 tracks plus 5 bonus
        for (var i = 1; i <= 10; i++)
        {
            CreateFile(sourceDir, $"00{i:D2} Track {i}.mp3", i * 1000);
        }
        for (var i = 11; i <= 15; i++)
        {
            CreateFile(sourceDir, $"00{i:D2} Bonus Track {i}.mp3", i * 1000);
        }

        var result = await MergeFilesAsync(sourceDir, targetDir, CancellationToken.None);

        Assert.Equal(5, result.FilesMoved); // 5 bonus tracks moved
        Assert.Equal(10, result.FilesDeleted); // 10 duplicate tracks deleted

        // Verify bonus tracks exist in target
        for (var i = 11; i <= 15; i++)
        {
            Assert.True(File.Exists(Path.Combine(targetDir, $"00{i:D2} Bonus Track {i}.mp3")));
        }

        // Verify source directory has no media files
        var remainingMediaFiles = Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories)
            .Count(f => FileHelper.IsFileMediaType(Path.GetExtension(f)));
        Assert.Equal(0, remainingMediaFiles);
    }

    #endregion

    #region Helper Methods

    private string CreateDirectory(string name)
    {
        var path = Path.Combine(_testDir, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private static void CreateFile(string directory, string fileName, int size)
    {
        var filePath = Path.Combine(directory, fileName);
        var content = new byte[size];
        new Random(size).NextBytes(content);
        File.WriteAllBytes(filePath, content);
    }

    /// <summary>
    /// Simulates the MergeFilesAsync method from AlbumFindDuplicateDirsCommand.
    /// This allows testing the merge logic in isolation.
    /// </summary>
    private static async Task<MergeResult> MergeFilesAsync(
        string sourceDir,
        string targetDir,
        CancellationToken cancellationToken)
    {
        var movedCount = 0;
        var deletedCount = 0;

        if (!Directory.Exists(targetDir))
        {
            return new MergeResult(0, 0);
        }

        var sourceFiles = Directory.EnumerateFiles(sourceDir, "*", SearchOption.TopDirectoryOnly)
            .Where(f => FileHelper.IsFileMediaType(Path.GetExtension(f)))
            .ToList();

        var targetFiles = Directory.EnumerateFiles(targetDir, "*", SearchOption.TopDirectoryOnly)
            .Select(f => Path.GetFileName(f).ToLowerInvariant())
            .ToHashSet();

        var targetFileSizes = Directory.EnumerateFiles(targetDir, "*", SearchOption.TopDirectoryOnly)
            .Where(f => FileHelper.IsFileMediaType(Path.GetExtension(f)))
            .Select(f => new FileInfo(f).Length)
            .ToHashSet();

        foreach (var sourceFile in sourceFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var fileName = Path.GetFileName(sourceFile);
            var targetPath = Path.Combine(targetDir, fileName);
            var sourceSize = new FileInfo(sourceFile).Length;

            // Check if file already exists in target (by name)
            if (targetFiles.Contains(fileName.ToLowerInvariant()))
            {
                File.Delete(sourceFile);
                deletedCount++;
                continue;
            }

            // Check if a file with the same content exists (by size comparison as quick check)
            if (targetFileSizes.Contains(sourceSize))
            {
                File.Delete(sourceFile);
                deletedCount++;
                continue;
            }

            // Generate unique name if conflict
            var finalTargetPath = targetPath;
            var counter = 1;
            while (File.Exists(finalTargetPath))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var ext = Path.GetExtension(fileName);
                finalTargetPath = Path.Combine(targetDir, $"{nameWithoutExt}_{counter}{ext}");
                counter++;
            }

            File.Move(sourceFile, finalTargetPath);
            movedCount++;
        }

        // Also merge non-media files (images, nfo, etc.)
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        var metadataExtensions = new[] { ".nfo", ".txt", ".cue", ".log", ".m3u", ".m3u8" };
        var allowedExtensions = imageExtensions.Concat(metadataExtensions).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sourceNonMediaFiles = Directory.EnumerateFiles(sourceDir, "*", SearchOption.TopDirectoryOnly)
            .Where(f => allowedExtensions.Contains(Path.GetExtension(f)))
            .ToList();

        var targetNonMediaFileNames = Directory.EnumerateFiles(targetDir, "*", SearchOption.TopDirectoryOnly)
            .Select(f => Path.GetFileName(f).ToLowerInvariant())
            .ToHashSet();

        foreach (var sourceFile in sourceNonMediaFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var fileName = Path.GetFileName(sourceFile);

            // Skip if target already has a file with this name
            if (targetNonMediaFileNames.Contains(fileName.ToLowerInvariant()))
            {
                File.Delete(sourceFile);
                continue;
            }

            var targetPath = Path.Combine(targetDir, fileName);
            File.Move(sourceFile, targetPath);
        }

        await Task.CompletedTask;
        return new MergeResult(movedCount, deletedCount);
    }

    private sealed record MergeResult(int FilesMoved, int FilesDeleted);

    #endregion
}
