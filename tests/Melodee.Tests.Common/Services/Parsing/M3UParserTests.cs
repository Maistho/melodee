using System.Text;
using Melodee.Common.Services.Parsing;
using Serilog.Core;

namespace Melodee.Tests.Common.Services.Parsing;

public class M3UParserTests
{
    private static M3UParser CreateParser()
    {
        return new M3UParser(Logger.None);
    }

    [Fact]
    public async Task ParseAsync_WithSimpleM3U_ParsesEntries()
    {
        var parser = CreateParser();
        var content = """
            #EXTM3U
            Artist/Album/01 - Song One.flac
            Artist/Album/02 - Song Two.flac
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Equal(2, result.Entries.Count);
        Assert.Equal("Artist/Album/01 - Song One.flac", result.Entries[0].NormalizedReference);
        Assert.Equal("Artist/Album/02 - Song Two.flac", result.Entries[1].NormalizedReference);
    }

    [Fact]
    public async Task ParseAsync_SkipsBlankLines_Successfully()
    {
        var parser = CreateParser();
        var content = """
            #EXTM3U
            
            Artist/Album/Song.flac
            
            
            Artist/Album/Song2.flac
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Equal(2, result.Entries.Count);
    }

    [Fact]
    public async Task ParseAsync_SkipsComments_Successfully()
    {
        var parser = CreateParser();
        var content = """
            #EXTM3U
            #EXTINF:123,Artist - Title
            Artist/Album/Song.flac
            # This is a comment
            Artist/Album/Song2.flac
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Equal(2, result.Entries.Count);
    }

    [Fact]
    public async Task ParseAsync_NormalizesBackslashes_ToForwardSlashes()
    {
        var parser = CreateParser();
        var content = @"D:\Music\Artist\Album\Song.mp3";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Single(result.Entries);
        Assert.Equal("D:/Music/Artist/Album/Song.mp3", result.Entries[0].NormalizedReference);
    }

    [Fact]
    public async Task ParseAsync_DecodesUrlEncodedPaths_Successfully()
    {
        var parser = CreateParser();
        var content = "Artist/Album/Song%20With%20Spaces.flac";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Single(result.Entries);
        Assert.Equal("Artist/Album/Song With Spaces.flac", result.Entries[0].NormalizedReference);
    }

    [Fact]
    public async Task ParseAsync_RemovesQuotes_FromPaths()
    {
        var parser = CreateParser();
        var content = """
            "Artist/Album/Song.flac"
            'Artist/Album/Song2.flac'
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Equal(2, result.Entries.Count);
        Assert.Equal("Artist/Album/Song.flac", result.Entries[0].NormalizedReference);
        Assert.Equal("Artist/Album/Song2.flac", result.Entries[1].NormalizedReference);
    }

    [Fact]
    public async Task ParseAsync_ExtractsPathHints_Correctly()
    {
        var parser = CreateParser();
        var content = "Pink Floyd/The Wall/02 - Another Brick in the Wall.flac";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Single(result.Entries);
        var entry = result.Entries[0];
        Assert.Equal("02 - Another Brick in the Wall.flac", entry.FileName);
        Assert.Equal("The Wall", entry.AlbumFolder);
        Assert.Equal("Pink Floyd", entry.ArtistFolder);
    }

    [Fact]
    public async Task ParseAsync_HandlesAbsolutePaths_WithMultipleSegments()
    {
        var parser = CreateParser();
        var content = "/mnt/music/Artist/Album/Song.flac";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Single(result.Entries);
        var entry = result.Entries[0];
        Assert.Equal("Song.flac", entry.FileName);
        Assert.Equal("Album", entry.AlbumFolder);
        Assert.Equal("Artist", entry.ArtistFolder);
    }

    [Fact]
    public async Task ParseAsync_HandlesSingleFileNameOnly_WithoutFolders()
    {
        var parser = CreateParser();
        var content = "song.flac";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Single(result.Entries);
        var entry = result.Entries[0];
        Assert.Equal("song.flac", entry.FileName);
        Assert.Null(entry.AlbumFolder);
        Assert.Null(entry.ArtistFolder);
    }

    [Fact]
    public async Task ParseAsync_HandlesM3U8_UsesUTF8Encoding()
    {
        var parser = CreateParser();
        var content = "Artist/Album/日本語のタイトル.flac";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u8");

        Assert.Single(result.Entries);
        Assert.Contains("日本語", result.Entries[0].NormalizedReference);
        Assert.Equal("utf-8", result.Encoding);
    }

    [Fact]
    public async Task ParseAsync_PreservesSortOrder_FromLineNumbers()
    {
        var parser = CreateParser();
        var content = """
            #EXTM3U
            First.flac
            Second.flac
            Third.flac
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Equal(3, result.Entries.Count);
        Assert.Equal(2, result.Entries[0].SortOrder);
        Assert.Equal(3, result.Entries[1].SortOrder);
        Assert.Equal(4, result.Entries[2].SortOrder);
    }

    [Fact]
    public async Task ParseAsync_HandlesUrlsAsEntries_WithoutCrashing()
    {
        var parser = CreateParser();
        var content = "http://example.com/stream.mp3";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Single(result.Entries);
        Assert.Equal("http://example.com/stream.mp3", result.Entries[0].NormalizedReference);
        Assert.Null(result.Entries[0].ArtistFolder);
        Assert.Null(result.Entries[0].AlbumFolder);
    }

    [Fact]
    public async Task ParseAsync_HandlesSpecialCharacters_InPaths()
    {
        var parser = CreateParser();
        var content = "Artist/Album [2024]/Song (Remastered).flac";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await parser.ParseAsync(stream, "test.m3u");

        Assert.Single(result.Entries);
        Assert.Equal("Artist/Album [2024]/Song (Remastered).flac", result.Entries[0].NormalizedReference);
        Assert.Equal("Song (Remastered).flac", result.Entries[0].FileName);
    }
}
