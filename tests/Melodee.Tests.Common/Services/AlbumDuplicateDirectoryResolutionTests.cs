using System.Text.RegularExpressions;
using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Services;

/// <summary>
/// Tests for album duplicate directory detection and resolution logic.
/// These tests verify the core algorithms used in the CLI find-duplicate-dirs command.
/// </summary>
public sealed class AlbumDuplicateDirectoryResolutionTests
{
    private static readonly Regex ArtistIdRegex = new(@"\s*\[\d+\]\s*$", RegexOptions.Compiled);
    private static readonly Regex YearRegex = new(@"\[(\d{4})\]|\((\d{4})\)|^(\d{4})\s", RegexOptions.Compiled);
    private static readonly Regex YearRemovalRegex = new(@"\s*[\[\(]?\d{4}[\]\)]?\s*", RegexOptions.Compiled);

    #region Artist ID Stripping Tests

    [Theory]
    [InlineData("Nanowar Of Steel [7590]", "Nanowar Of Steel")]
    [InlineData("Paul Simon [2971]", "Paul Simon")]
    [InlineData("AC/DC [7]", "AC/DC")]
    [InlineData("Pink Floyd [5891]", "Pink Floyd")]
    [InlineData("The Beatles [123456]", "The Beatles")]
    [InlineData("Simple Artist", "Simple Artist")]
    [InlineData("Artist [12345] Name", "Artist [12345] Name")] // ID must be at end
    [InlineData("Artist Name [not-a-number]", "Artist Name [not-a-number]")] // Must be numeric
    [InlineData("Artist [123] [456]", "Artist [123]")] // Only strips last ID
    public void StripArtistId_VariousFormats_ReturnsExpectedResult(string input, string expected)
    {
        var result = ArtistIdRegex.Replace(input, string.Empty).Trim();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void StripArtistId_EmptyString_ReturnsEmpty()
    {
        var result = ArtistIdRegex.Replace(string.Empty, string.Empty).Trim();
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void StripArtistId_OnlyId_ReturnsEmpty()
    {
        var result = ArtistIdRegex.Replace("[12345]", string.Empty).Trim();
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region Album Year Parsing Tests

    [Theory]
    [InlineData("[1985] Gold River", 1985, "Gold River")]
    [InlineData("[2010] Gold River", 2010, "Gold River")]
    [InlineData("(1999) Some Album", 1999, "Some Album")]
    [InlineData("2001 Space Odyssey", 2001, "Space Odyssey")]
    [InlineData("[1973] The Dark Side Of The Moon", 1973, "The Dark Side Of The Moon")]
    [InlineData("Album Without Year", null, "Album Without Year")]
    [InlineData("[2025] Future Album", 2025, "Future Album")]
    public void ParseYearFromDirectoryName_VariousFormats_ReturnsExpectedResult(
        string dirName, int? expectedYear, string expectedAlbumName)
    {
        var (year, albumName) = ParseAlbumDirectoryName(dirName);
        Assert.Equal(expectedYear, year);
        Assert.Equal(expectedAlbumName, albumName.Trim());
    }

    [Fact]
    public void ParseYearFromDirectoryName_InvalidYear_ReturnsNull()
    {
        var (year, _) = ParseAlbumDirectoryName("[1800] Too Old Album");
        Assert.Null(year); // Year before 1900 is invalid
    }

    [Fact]
    public void ParseYearFromDirectoryName_FutureYear_ReturnsNull()
    {
        var currentYear = DateTime.Now.Year;
        var (year, _) = ParseAlbumDirectoryName($"[{currentYear + 10}] Far Future Album");
        Assert.Null(year); // Year too far in future is invalid
    }

    #endregion

    #region Duplicate Detection Tests

    [Fact]
    public void DetectDuplicates_SameNormalizedName_GroupedTogether()
    {
        var directories = new[]
        {
            new AlbumDirectoryInfo("[1985] Gold River", "Gold River", 1985),
            new AlbumDirectoryInfo("[2010] Gold River", "Gold River", 2010),
            new AlbumDirectoryInfo("[1990] Different Album", "Different Album", 1990)
        };

        var groups = GroupDuplicatesByNormalizedName(directories, "Test Artist");

        Assert.Single(groups.Where(g => g.Directories.Count > 1));
        var duplicateGroup = groups.First(g => g.Directories.Count > 1);
        Assert.Equal(2, duplicateGroup.Directories.Count);
        Assert.Equal("Gold River", duplicateGroup.AlbumName);
    }

    [Fact]
    public void DetectDuplicates_DifferentSpacing_GroupedTogether()
    {
        var directories = new[]
        {
            new AlbumDirectoryInfo("[1997] Tri State", "Tri State", 1997),
            new AlbumDirectoryInfo("[1997] TriState", "TriState", 1997)
        };

        var groups = GroupDuplicatesByNormalizedName(directories, "Above And Beyond");

        Assert.Single(groups);
        Assert.Equal(2, groups[0].Directories.Count);
    }

    [Fact]
    public void DetectDuplicates_DifferentPunctuation_GroupedTogether()
    {
        var directories = new[]
        {
            new AlbumDirectoryInfo("[1997] So Long So Wrong", "So Long So Wrong", 1997),
            new AlbumDirectoryInfo("[1997] So Long, So Wrong", "So Long, So Wrong", 1997)
        };

        var groups = GroupDuplicatesByNormalizedName(directories, "Alison Krauss");

        Assert.Single(groups);
        Assert.Equal(2, groups[0].Directories.Count);
    }

    [Fact]
    public void DetectDuplicates_SingleDirectory_NotGroupedAsDuplicate()
    {
        var directories = new[]
        {
            new AlbumDirectoryInfo("[1985] Unique Album", "Unique Album", 1985)
        };

        var groups = GroupDuplicatesByNormalizedName(directories, "Test Artist");
        var duplicateGroups = groups.Where(g => g.Directories.Count > 1).ToList();

        Assert.Empty(duplicateGroups);
    }

    #endregion

    #region Resolution Logic Tests

    [Fact]
    public void ResolveWithMetadata_OneMatchesYear_SetsCorrectTarget()
    {
        var group = new DuplicateAlbumGroup
        {
            ArtistName = "Accept",
            AlbumName = "Balls to the Wall",
            Directories =
            [
                new AlbumDirectoryInfo("[1983] Balls To The Wall", "Balls To The Wall", 1983),
                new AlbumDirectoryInfo("[1984] Balls To The Wall", "Balls To The Wall", 1984)
            ]
        };

        ResolveWithMetadataYear(group, 1983);

        Assert.Equal(1983, group.MetadataYear);
        Assert.Equal("[1983] Balls To The Wall", group.SuggestedTargetDirectory);
        Assert.Single(group.SuggestedMergeDirectories!);
        Assert.Equal("[1984] Balls To The Wall", group.SuggestedMergeDirectories![0]);
    }

    [Fact]
    public void ResolveWithMetadata_NoneMatchYear_NoTargetOrMergeSet()
    {
        var group = new DuplicateAlbumGroup
        {
            ArtistName = "Accept",
            AlbumName = "Balls to the Wall",
            Directories =
            [
                new AlbumDirectoryInfo("[1983] Balls To The Wall", "Balls To The Wall", 1983),
                new AlbumDirectoryInfo("[1984] Balls To The Wall", "Balls To The Wall", 1984)
            ]
        };

        ResolveWithMetadataYear(group, 1982); // Metadata says 1982, but no directory has that year

        Assert.Equal(1982, group.MetadataYear);
        Assert.Null(group.SuggestedTargetDirectory); // No directory matches
        Assert.Null(group.SuggestedMergeDirectories); // No merge without a target
    }

    [Fact]
    public void ResolveWithMetadata_MultipleMatchYear_BestMatchIsTarget_OthersMerge()
    {
        var group = new DuplicateAlbumGroup
        {
            ArtistName = "Black Sabbath",
            AlbumName = "Paranoid",
            Directories =
            [
                new AlbumDirectoryInfo("[1970] Paranoid", "Paranoid", 1970, 8),
                new AlbumDirectoryInfo("_duplicate_ [1970] Paranoid", "Paranoid", 1970, 11),
                new AlbumDirectoryInfo("[2016] Paranoid", "Paranoid", 2016, 16)
            ]
        };

        ResolveWithMetadataYear(group, 1970);

        Assert.Equal(1970, group.MetadataYear);
        // The first matching directory (non-duplicate prefix, correct year) should be target
        Assert.Equal("[1970] Paranoid", group.SuggestedTargetDirectory);
        // ALL other directories should be merged, including duplicate with correct year
        Assert.Equal(2, group.SuggestedMergeDirectories!.Length);
        Assert.Contains("_duplicate_ [1970] Paranoid", group.SuggestedMergeDirectories);
        Assert.Contains("[2016] Paranoid", group.SuggestedMergeDirectories);
    }

    [Fact]
    public void ResolveWithMetadata_ThreeDirectoriesTwoMerge_CorrectMergeList()
    {
        var group = new DuplicateAlbumGroup
        {
            ArtistName = "Cancer",
            AlbumName = "Death Shall Rise",
            Directories =
            [
                new AlbumDirectoryInfo("[1991] Death Shall Rise", "Death Shall Rise", 1991),
                new AlbumDirectoryInfo("[2014] Death Shall Rise", "Death Shall Rise", 2014),
                new AlbumDirectoryInfo("[2021] Death Shall Rise", "Death Shall Rise", 2021)
            ]
        };

        ResolveWithMetadataYear(group, 1991);

        Assert.Equal("[1991] Death Shall Rise", group.SuggestedTargetDirectory);
        Assert.Equal(2, group.SuggestedMergeDirectories!.Length);
        Assert.Contains("[2014] Death Shall Rise", group.SuggestedMergeDirectories);
        Assert.Contains("[2021] Death Shall Rise", group.SuggestedMergeDirectories);
    }

    #endregion

    #region Normalized Name Tests

    [Theory]
    [InlineData("The Dark Side Of The Moon", "THEDARKSIDEOFTHEMOON")]
    [InlineData("What's the Story Morning Glory?", "WHATSTHESTORYMORNINGGLORY")]
    [InlineData("AC/DC", "ACDC")]
    [InlineData("Guns N' Roses", "GUNSNROSES")]
    [InlineData("...And Justice For All", "ANDJUSTICEFORALL")]
    public void ToNormalizedString_VariousFormats_ReturnsExpectedResult(string input, string expected)
    {
        var result = input.ToNormalizedString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Its Too Late To Stop Now", "It's Too Late to Stop Now", true)] // Directory vs Search result
    [InlineData("Whats The Story Morning Glory", "(What's the Story) Morning Glory?", true)]
    [InlineData("Peace Sellsbut Whos Buying", "Peace Sells...But Who's Buying", true)]
    [InlineData("Completely Different Album", "Peace Sells", false)]
    public void ToNormalizedString_MatchesSearchResults(string directoryName, string searchResultName, bool shouldMatch)
    {
        var dirNormalized = directoryName.ToNormalizedString();
        var searchNormalized = searchResultName.ToNormalizedString();
        
        if (shouldMatch)
        {
            Assert.Equal(dirNormalized, searchNormalized);
        }
        else
        {
            Assert.NotEqual(dirNormalized, searchNormalized);
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void DetectDuplicates_EmptyList_ReturnsEmpty()
    {
        var directories = Array.Empty<AlbumDirectoryInfo>();
        var groups = GroupDuplicatesByNormalizedName(directories, "Test Artist");
        Assert.Empty(groups);
    }

    [Fact]
    public void ResolveWithMetadata_DirectoryWithNoYear_HandledCorrectly()
    {
        var group = new DuplicateAlbumGroup
        {
            ArtistName = "Test Artist",
            AlbumName = "Test Album",
            Directories =
            [
                new AlbumDirectoryInfo("[1990] Test Album", "Test Album", 1990),
                new AlbumDirectoryInfo("Test Album", "Test Album", null) // No year in directory name
            ]
        };

        ResolveWithMetadataYear(group, 1990);

        Assert.Equal("[1990] Test Album", group.SuggestedTargetDirectory);
        Assert.Single(group.SuggestedMergeDirectories!);
    }

    [Fact]
    public void ResolveWithMetadata_AllDirectoriesHaveNoYear_NoResolution()
    {
        var group = new DuplicateAlbumGroup
        {
            ArtistName = "Test Artist",
            AlbumName = "Test Album",
            Directories =
            [
                new AlbumDirectoryInfo("Test Album", "Test Album", null),
                new AlbumDirectoryInfo("Test Album Copy", "Test Album", null)
            ]
        };

        ResolveWithMetadataYear(group, 1990);

        Assert.Equal(1990, group.MetadataYear);
        Assert.Null(group.SuggestedTargetDirectory); // No directory matches
        Assert.Null(group.SuggestedMergeDirectories); // No merge without a target
    }

    #endregion

    #region Helper Methods (Simulating CLI Logic)

    private static (int? year, string albumName) ParseAlbumDirectoryName(string dirName)
    {
        var albumName = dirName;
        int? year = null;

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
            }
        }

        return (year, albumName);
    }

    private static List<DuplicateAlbumGroup> GroupDuplicatesByNormalizedName(
        AlbumDirectoryInfo[] directories,
        string artistName)
    {
        var groups = directories
            .GroupBy(d => d.AlbumName.ToNormalizedString())
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateAlbumGroup
            {
                ArtistName = artistName,
                AlbumName = g.First().AlbumName,
                Directories = g.OrderBy(x => x.Year).ToList()
            })
            .ToList();

        return groups;
    }

    private static void ResolveWithMetadataYear(DuplicateAlbumGroup group, int metadataYear)
    {
        group.MetadataYear = metadataYear;
        group.MetadataSource = "Test";

        foreach (var dir in group.Directories)
        {
            dir.IsCorrectYear = dir.Year == metadataYear;
        }

        // Find the best target directory:
        // 1. Must have correct year
        // 2. Prefer non-duplicate prefixed directories
        // 3. If all have correct year, prefer the one with most files (likely has bonus tracks)
        var targetDir = group.Directories
            .Where(d => d.IsCorrectYear == true)
            .OrderBy(d => d.Path.Contains("_duplicate_", StringComparison.OrdinalIgnoreCase) ? 1 : 0) // Non-duplicate first
            .ThenByDescending(d => d.FileCount) // More files preferred
            .FirstOrDefault();

        if (targetDir != null)
        {
            targetDir.IsTarget = true;
            group.SuggestedTargetDirectory = targetDir.Path;
            
            // ALL other directories (regardless of year) should be merged into target
            group.SuggestedMergeDirectories = group.Directories
                .Where(d => d != targetDir) // All except the target
                .Select(d => d.Path)
                .ToArray();
        }
        else
        {
            // No directory has the correct year - don't set any target or merge directories
            group.SuggestedTargetDirectory = null;
            group.SuggestedMergeDirectories = null;
        }
    }

    #endregion

    #region Test Data Classes

    private sealed class AlbumDirectoryInfo
    {
        public string Path { get; }
        public string AlbumName { get; }
        public int? Year { get; }
        public int FileCount { get; }
        public bool? IsCorrectYear { get; set; }
        public bool IsTarget { get; set; }

        public AlbumDirectoryInfo(string path, string albumName, int? year, int fileCount = 10)
        {
            Path = path;
            AlbumName = albumName;
            Year = year;
            FileCount = fileCount;
        }
    }

    private sealed class DuplicateAlbumGroup
    {
        public required string ArtistName { get; init; }
        public required string AlbumName { get; init; }
        public List<AlbumDirectoryInfo> Directories { get; init; } = [];
        public int? MetadataYear { get; set; }
        public string? MetadataSource { get; set; }
        public string? SuggestedTargetDirectory { get; set; }
        public string[]? SuggestedMergeDirectories { get; set; }
    }

    #endregion
}
