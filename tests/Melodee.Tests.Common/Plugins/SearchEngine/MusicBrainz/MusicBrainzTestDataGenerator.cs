using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Plugins.SearchEngine.MusicBrainz;

/// <summary>
/// Generates synthetic MusicBrainz TSV test data for unit testing and benchmarking.
/// Produces files in the same tab-separated format as actual MusicBrainz dumps.
/// </summary>
public static class MusicBrainzTestDataGenerator
{
    private static readonly string[] FirstNames =
    [
        "John", "Paul", "George", "Ringo", "David", "Robert", "James", "Michael",
        "William", "Richard", "Thomas", "Charles", "Daniel", "Matthew", "Anthony",
        "Mark", "Steven", "Andrew", "Joshua", "Kenneth", "Kevin", "Brian", "Edward",
        "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan", "Jacob", "Gary"
    ];

    private static readonly string[] LastNames =
    [
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
        "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson",
        "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson"
    ];

    private static readonly string[] BandSuffixes =
    [
        "", " Band", " Project", " Experience", " Collective", " Orchestra",
        " Quartet", " Trio", " Ensemble", " Group"
    ];

    private static readonly string[] AlbumTitles =
    [
        "First Album", "Debut", "The Beginning", "Origins", "Genesis", "Chapter One",
        "Breakthrough", "Rising", "Evolution", "Revolution", "Metamorphosis",
        "Transcendence", "Echoes", "Reflections", "Shadows", "Light", "Darkness",
        "Journey", "Voyage", "Odyssey", "Chronicles", "Tales", "Stories", "Dreams",
        "Visions", "Horizons", "Boundaries", "Limits", "Beyond", "Infinity"
    ];

    private static readonly string[] AlbumAdjectives =
    [
        "The", "A", "My", "Our", "Their", "Lost", "Found", "Hidden", "Secret",
        "Eternal", "Infinite", "Final", "First", "Last", "New", "Old", "Ancient",
        "Modern", "Future", "Past", "Present", "Golden", "Silver", "Dark", "Bright"
    ];

    /// <summary>
    /// Generates a complete set of MusicBrainz test data files.
    /// </summary>
    /// <param name="outputPath">Directory to write TSV files</param>
    /// <param name="artistCount">Number of artists to generate</param>
    /// <param name="albumsPerArtist">Average albums per artist</param>
    /// <param name="aliasesPerArtist">Average aliases per artist</param>
    /// <param name="seed">Random seed for reproducibility</param>
    public static TestDataStats GenerateTestData(
        string outputPath,
        int artistCount = 1000,
        int albumsPerArtist = 5,
        int aliasesPerArtist = 2,
        int seed = 42)
    {
        Directory.CreateDirectory(outputPath);

        var random = new Random(seed);
        var stats = new TestDataStats();

        // Generate artists
        var artists = GenerateArtists(random, artistCount);
        stats.ArtistCount = artists.Count;

        // Generate artist aliases
        var aliases = GenerateArtistAliases(random, artists, aliasesPerArtist);
        stats.AliasCount = aliases.Count;

        // Generate artist credits (one per artist for simplicity)
        var artistCredits = GenerateArtistCredits(artists);
        stats.ArtistCreditCount = artistCredits.Count;

        // Generate artist credit names
        var artistCreditNames = GenerateArtistCreditNames(artistCredits, artists);
        stats.ArtistCreditNameCount = artistCreditNames.Count;

        // Generate release groups (albums)
        var releaseGroups = GenerateReleaseGroups(random, artists, albumsPerArtist);
        stats.ReleaseGroupCount = releaseGroups.Count;

        // Generate release group meta (dates)
        var releaseGroupMetas = GenerateReleaseGroupMetas(random, releaseGroups);
        stats.ReleaseGroupMetaCount = releaseGroupMetas.Count;

        // Generate releases
        var releases = GenerateReleases(random, releaseGroups);
        stats.ReleaseCount = releases.Count;

        // Generate release countries
        var releaseCountries = GenerateReleaseCountries(random, releases);
        stats.ReleaseCountryCount = releaseCountries.Count;

        // Generate links (for artist relations)
        var links = GenerateLinks(random, artistCount / 10);
        stats.LinkCount = links.Count;

        // Generate artist-to-artist links
        var artistLinks = GenerateArtistToArtistLinks(random, artists, links);
        stats.ArtistLinkCount = artistLinks.Count;

        // Write all files
        WriteArtistFile(Path.Combine(outputPath, "artist"), artists);
        WriteArtistAliasFile(Path.Combine(outputPath, "artist_alias"), aliases);
        WriteArtistCreditFile(Path.Combine(outputPath, "artist_credit"), artistCredits);
        WriteArtistCreditNameFile(Path.Combine(outputPath, "artist_credit_name"), artistCreditNames);
        WriteReleaseGroupFile(Path.Combine(outputPath, "release_group"), releaseGroups);
        WriteReleaseGroupMetaFile(Path.Combine(outputPath, "release_group_meta"), releaseGroupMetas);
        WriteReleaseFile(Path.Combine(outputPath, "release"), releases);
        WriteReleaseCountryFile(Path.Combine(outputPath, "release_country"), releaseCountries);
        WriteLinkFile(Path.Combine(outputPath, "link"), links);
        WriteArtistToArtistLinkFile(Path.Combine(outputPath, "l_artist_artist"), artistLinks);

        return stats;
    }

    private static List<TestArtist> GenerateArtists(Random random, int count)
    {
        var artists = new List<TestArtist>(count);
        for (var i = 1; i <= count; i++)
        {
            var name = GenerateArtistName(random);
            artists.Add(new TestArtist
            {
                Id = i,
                MusicBrainzId = GenerateDeterministicGuid(random),
                Name = name,
                SortName = GenerateSortName(name)
            });
        }
        return artists;
    }

    private static Guid GenerateDeterministicGuid(Random random)
    {
        var bytes = new byte[16];
        random.NextBytes(bytes);
        return new Guid(bytes);
    }

    private static string GenerateArtistName(Random random)
    {
        if (random.Next(3) == 0)
        {
            return $"{FirstNames[random.Next(FirstNames.Length)]} {LastNames[random.Next(LastNames.Length)]}{BandSuffixes[random.Next(BandSuffixes.Length)]}";
        }

        return $"The {LastNames[random.Next(LastNames.Length)]}{BandSuffixes[random.Next(BandSuffixes.Length)]}".Trim();
    }

    private static string GenerateSortName(string name)
    {
        if (name.StartsWith("The "))
        {
            return name[4..] + ", The";
        }
        return name;
    }

    private static List<TestArtistAlias> GenerateArtistAliases(Random random, List<TestArtist> artists, int avgPerArtist)
    {
        var aliases = new List<TestArtistAlias>();
        var id = 1;
        foreach (var artist in artists)
        {
            var aliasCount = random.Next(0, avgPerArtist * 2);
            for (var i = 0; i < aliasCount; i++)
            {
                aliases.Add(new TestArtistAlias
                {
                    Id = id++,
                    ArtistId = artist.Id,
                    Name = GenerateAliasName(random, artist.Name)
                });
            }
        }
        return aliases;
    }

    private static string GenerateAliasName(Random random, string artistName)
    {
        return random.Next(3) switch
        {
            0 => artistName.ToUpperInvariant(),
            1 => artistName.Replace(" ", ""),
            _ => $"{artistName} ({random.Next(1960, 2025)})"
        };
    }

    private static List<TestArtistCredit> GenerateArtistCredits(List<TestArtist> artists)
    {
        return artists.Select(a => new TestArtistCredit
        {
            Id = a.Id,
            ArtistCount = 1
        }).ToList();
    }

    private static List<TestArtistCreditName> GenerateArtistCreditNames(
        List<TestArtistCredit> credits, List<TestArtist> artists)
    {
        var names = new List<TestArtistCreditName>();
        var id = 1;
        foreach (var credit in credits)
        {
            var artist = artists.First(a => a.Id == credit.Id);
            names.Add(new TestArtistCreditName
            {
                Id = id++,
                ArtistCreditId = credit.Id,
                Position = 0,
                ArtistId = artist.Id
            });
        }
        return names;
    }

    private static List<TestReleaseGroup> GenerateReleaseGroups(
        Random random, List<TestArtist> artists, int avgPerArtist)
    {
        var groups = new List<TestReleaseGroup>();
        var id = 1;
        foreach (var artist in artists)
        {
            var albumCount = random.Next(1, avgPerArtist * 2);
            for (var i = 0; i < albumCount; i++)
            {
                groups.Add(new TestReleaseGroup
                {
                    Id = id++,
                    MusicBrainzId = GenerateDeterministicGuid(random),
                    ArtistCreditId = artist.Id,
                    ReleaseType = random.Next(1, 12)
                });
            }
        }
        return groups;
    }

    private static List<TestReleaseGroupMeta> GenerateReleaseGroupMetas(
        Random random, List<TestReleaseGroup> groups)
    {
        return groups.Select(g =>
        {
            var year = random.Next(1950, 2025);
            var month = random.Next(1, 13);
            var day = random.Next(1, 29);
            return new TestReleaseGroupMeta
            {
                ReleaseGroupId = g.Id,
                Year = year,
                Month = month,
                Day = day
            };
        }).ToList();
    }

    private static List<TestRelease> GenerateReleases(Random random, List<TestReleaseGroup> groups)
    {
        var releases = new List<TestRelease>();
        var id = 1;
        foreach (var group in groups)
        {
            var releaseCount = random.Next(1, 3);
            for (var i = 0; i < releaseCount; i++)
            {
                var name = GenerateAlbumName(random);
                releases.Add(new TestRelease
                {
                    Id = id++,
                    MusicBrainzId = GenerateDeterministicGuid(random),
                    Name = name,
                    ArtistCreditId = group.ArtistCreditId,
                    ReleaseGroupId = group.Id
                });
            }
        }
        return releases;
    }

    private static string GenerateAlbumName(Random random)
    {
        if (random.Next(2) == 0)
        {
            return $"{AlbumAdjectives[random.Next(AlbumAdjectives.Length)]} {AlbumTitles[random.Next(AlbumTitles.Length)]}";
        }
        return AlbumTitles[random.Next(AlbumTitles.Length)];
    }

    private static List<TestReleaseCountry> GenerateReleaseCountries(
        Random random, List<TestRelease> releases)
    {
        var countries = new List<TestReleaseCountry>();
        var id = 1;
        foreach (var release in releases)
        {
            if (random.Next(2) == 0)
            {
                var year = random.Next(1950, 2025);
                countries.Add(new TestReleaseCountry
                {
                    Id = id++,
                    ReleaseId = release.Id,
                    Year = year,
                    Month = random.Next(1, 13),
                    Day = random.Next(1, 29)
                });
            }
        }
        return countries;
    }

    private static List<TestLink> GenerateLinks(Random random, int count)
    {
        var links = new List<TestLink>();
        for (var i = 1; i <= count; i++)
        {
            var beginYear = random.Next(1950, 2020);
            links.Add(new TestLink
            {
                Id = i,
                BeginYear = beginYear,
                BeginMonth = random.Next(1, 13),
                BeginDay = random.Next(1, 29),
                EndYear = random.Next(2) == 0 ? beginYear + random.Next(1, 20) : 0,
                EndMonth = random.Next(1, 13),
                EndDay = random.Next(1, 29)
            });
        }
        return links;
    }

    private static List<TestArtistToArtistLink> GenerateArtistToArtistLinks(
        Random random, List<TestArtist> artists, List<TestLink> links)
    {
        var artistLinks = new List<TestArtistToArtistLink>();
        var id = 1;
        foreach (var link in links)
        {
            var artist0 = artists[random.Next(artists.Count)];
            var artist1 = artists[random.Next(artists.Count)];
            if (artist0.Id != artist1.Id)
            {
                artistLinks.Add(new TestArtistToArtistLink
                {
                    Id = id++,
                    LinkId = link.Id,
                    Artist0 = artist0.Id,
                    Artist1 = artist1.Id,
                    LinkOrder = 0
                });
            }
        }
        return artistLinks;
    }

    #region File Writers

    private static void WriteArtistFile(string path, List<TestArtist> artists)
    {
        using var writer = new StreamWriter(path);
        foreach (var artist in artists)
        {
            // Format: id, gid, name, sort_name, begin_date_year, begin_date_month, begin_date_day, ...
            writer.WriteLine($"{artist.Id}\t{artist.MusicBrainzId}\t{artist.Name}\t{artist.SortName}\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N");
        }
    }

    private static void WriteArtistAliasFile(string path, List<TestArtistAlias> aliases)
    {
        using var writer = new StreamWriter(path);
        foreach (var alias in aliases)
        {
            // Format: id, artist_id, name, ...
            writer.WriteLine($"{alias.Id}\t{alias.ArtistId}\t{alias.Name}\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N");
        }
    }

    private static void WriteArtistCreditFile(string path, List<TestArtistCredit> credits)
    {
        using var writer = new StreamWriter(path);
        foreach (var credit in credits)
        {
            // Format: id, name, artist_count, ...
            writer.WriteLine($"{credit.Id}\tCredit {credit.Id}\t{credit.ArtistCount}\t\\N\t\\N");
        }
    }

    private static void WriteArtistCreditNameFile(string path, List<TestArtistCreditName> names)
    {
        using var writer = new StreamWriter(path);
        foreach (var name in names)
        {
            // Format: artist_credit_id, position, artist_id, name, join_phrase
            writer.WriteLine($"{name.ArtistCreditId}\t{name.Position}\t{name.ArtistId}\tName {name.Id}\t");
        }
    }

    private static void WriteReleaseGroupFile(string path, List<TestReleaseGroup> groups)
    {
        using var writer = new StreamWriter(path);
        foreach (var group in groups)
        {
            // Format: id, gid, name, artist_credit_id, type, ...
            writer.WriteLine($"{group.Id}\t{group.MusicBrainzId}\tRelease Group {group.Id}\t{group.ArtistCreditId}\t{group.ReleaseType}\t\\N\t\\N");
        }
    }

    private static void WriteReleaseGroupMetaFile(string path, List<TestReleaseGroupMeta> metas)
    {
        using var writer = new StreamWriter(path);
        foreach (var meta in metas)
        {
            // Format: release_group_id, release_count, first_release_date_year, month, day
            writer.WriteLine($"{meta.ReleaseGroupId}\t1\t{meta.Year}\t{meta.Month}\t{meta.Day}");
        }
    }

    private static void WriteReleaseFile(string path, List<TestRelease> releases)
    {
        using var writer = new StreamWriter(path);
        foreach (var release in releases)
        {
            var sortName = release.Name.CleanString(doTitleCase: true) ?? release.Name;
            // Format: id, gid, name, artist_credit_id, release_group_id, ...
            writer.WriteLine($"{release.Id}\t{release.MusicBrainzId}\t{release.Name}\t{release.ArtistCreditId}\t{release.ReleaseGroupId}\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N\t\\N");
        }
    }

    private static void WriteReleaseCountryFile(string path, List<TestReleaseCountry> countries)
    {
        using var writer = new StreamWriter(path);
        foreach (var country in countries)
        {
            // Format: release_id, country, date_year, date_month, date_day
            writer.WriteLine($"{country.ReleaseId}\t222\t{country.Year}\t{country.Month}\t{country.Day}");
        }
    }

    private static void WriteLinkFile(string path, List<TestLink> links)
    {
        using var writer = new StreamWriter(path);
        foreach (var link in links)
        {
            // Format: id, link_type, begin_year, begin_month, begin_day, end_year, end_month, end_day, ...
            writer.WriteLine($"{link.Id}\t1\t{link.BeginYear}\t{link.BeginMonth}\t{link.BeginDay}\t{link.EndYear}\t{link.EndMonth}\t{link.EndDay}\t\\N\t\\N");
        }
    }

    private static void WriteArtistToArtistLinkFile(string path, List<TestArtistToArtistLink> links)
    {
        using var writer = new StreamWriter(path);
        foreach (var link in links)
        {
            // Format: id, link_id, entity0, entity1, edits_pending, last_updated, link_order, ...
            writer.WriteLine($"{link.Id}\t{link.LinkId}\t{link.Artist0}\t{link.Artist1}\t0\t\\N\t{link.LinkOrder}\t\\N\t\\N");
        }
    }

    #endregion

    #region Test Data Models

    private sealed class TestArtist
    {
        public long Id { get; init; }
        public Guid MusicBrainzId { get; init; }
        public required string Name { get; init; }
        public required string SortName { get; init; }
    }

    private sealed class TestArtistAlias
    {
        public long Id { get; init; }
        public long ArtistId { get; init; }
        public required string Name { get; init; }
    }

    private sealed class TestArtistCredit
    {
        public long Id { get; init; }
        public int ArtistCount { get; init; }
    }

    private sealed class TestArtistCreditName
    {
        public long Id { get; init; }
        public long ArtistCreditId { get; init; }
        public int Position { get; init; }
        public long ArtistId { get; init; }
    }

    private sealed class TestReleaseGroup
    {
        public long Id { get; init; }
        public Guid MusicBrainzId { get; init; }
        public long ArtistCreditId { get; init; }
        public int ReleaseType { get; init; }
    }

    private sealed class TestReleaseGroupMeta
    {
        public long ReleaseGroupId { get; init; }
        public int Year { get; init; }
        public int Month { get; init; }
        public int Day { get; init; }
    }

    private sealed class TestRelease
    {
        public long Id { get; init; }
        public Guid MusicBrainzId { get; init; }
        public required string Name { get; init; }
        public long ArtistCreditId { get; init; }
        public long ReleaseGroupId { get; init; }
    }

    private sealed class TestReleaseCountry
    {
        public long Id { get; init; }
        public long ReleaseId { get; init; }
        public int Year { get; init; }
        public int Month { get; init; }
        public int Day { get; init; }
    }

    private sealed class TestLink
    {
        public long Id { get; init; }
        public int BeginYear { get; init; }
        public int BeginMonth { get; init; }
        public int BeginDay { get; init; }
        public int EndYear { get; init; }
        public int EndMonth { get; init; }
        public int EndDay { get; init; }
    }

    private sealed class TestArtistToArtistLink
    {
        public long Id { get; init; }
        public long LinkId { get; init; }
        public long Artist0 { get; init; }
        public long Artist1 { get; init; }
        public int LinkOrder { get; init; }
    }

    #endregion
}

/// <summary>
/// Statistics about generated test data.
/// </summary>
public sealed class TestDataStats
{
    public int ArtistCount { get; set; }
    public int AliasCount { get; set; }
    public int ArtistCreditCount { get; set; }
    public int ArtistCreditNameCount { get; set; }
    public int ReleaseGroupCount { get; set; }
    public int ReleaseGroupMetaCount { get; set; }
    public int ReleaseCount { get; set; }
    public int ReleaseCountryCount { get; set; }
    public int LinkCount { get; set; }
    public int ArtistLinkCount { get; set; }

    public override string ToString()
    {
        return $"Artists: {ArtistCount:N0}, Aliases: {AliasCount:N0}, " +
               $"ReleaseGroups: {ReleaseGroupCount:N0}, Releases: {ReleaseCount:N0}, " +
               $"Links: {LinkCount:N0}";
    }
}
