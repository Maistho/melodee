using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class SongExtensionsTitleHasUnwantedTextTests
{
    private Song CreateSong(string? title, int songNumber = 1, string? albumTitle = null)
    {
        var tags = new List<MetaTag<object?>>();

        if (title != null)
        {
            tags.Add(new MetaTag<object?> { Identifier = MetaTagIdentifier.Title, Value = title });
        }

        tags.Add(new MetaTag<object?> { Identifier = MetaTagIdentifier.TrackNumber, Value = songNumber });

        if (albumTitle != null)
        {
            tags.Add(new MetaTag<object?> { Identifier = MetaTagIdentifier.Album, Value = albumTitle });
        }

        return new Song
        {
            Tags = tags.ToArray(),
            File = new FileSystemFileInfo { Name = "test.mp3", Size = 1000 },
            CrcHash = string.Empty
        };
    }

    [Fact]
    public void TitleHasUnwantedText_NullTitle_ReturnsTrue()
    {
        var song = CreateSong(null);

        var result = song.TitleHasUnwantedText();

        Assert.True(result);
    }

    [Fact]
    public void TitleHasUnwantedText_EmptyTitle_ReturnsTrue()
    {
        var song = CreateSong("");

        var result = song.TitleHasUnwantedText();

        Assert.True(result);
    }

    [Fact]
    public void TitleHasUnwantedText_WhitespaceTitle_ReturnsTrue()
    {
        var song = CreateSong("   ");

        var result = song.TitleHasUnwantedText();

        Assert.True(result);
    }

    [Theory]
    [InlineData("Song feat. Artist")]
    [InlineData("Song ft. Artist")]
    [InlineData("Song featuring Artist")]
    [InlineData("Song (feat. Artist)")]
    [InlineData("Song [ft. Artist]")]
    public void TitleHasUnwantedText_FeaturingFragments_ReturnsTrue(string title)
    {
        var song = CreateSong(title);

        var result = song.TitleHasUnwantedText();

        Assert.True(result);
    }

    [Fact]
    public void TitleHasUnwantedText_MultipleSpaces_ReturnsTrue()
    {
        var song = CreateSong("Song  Title");

        var result = song.TitleHasUnwantedText();

        Assert.True(result);
    }

    [Fact]
    public void TitleHasUnwantedText_ProdWithSpace_ReturnsTrue()
    {
        var song = CreateSong("Song (prod Artist)");

        var result = song.TitleHasUnwantedText();

        Assert.True(result);
    }

    [Fact]
    public void TitleHasUnwantedText_TitleIsSongNumber_ReturnsTrue()
    {
        var song = CreateSong("5", 5);

        var result = song.TitleHasUnwantedText();

        Assert.True(result);
    }

    [Fact]
    public void TitleHasUnwantedText_TitleIsSongNumberWithSpaces_ReturnsTrue()
    {
        var song = CreateSong("  3  ", 3);

        var result = song.TitleHasUnwantedText();

        Assert.True(result);
    }

    [Fact]
    public void TitleHasUnwantedText_TitleStartsWithSongNumber_ReturnsTrue()
    {
        var song = CreateSong("01 Song Title", 1);

        var result = song.TitleHasUnwantedText();

        Assert.True(result);
    }

    [Fact]
    public void TitleHasUnwantedText_TitleStartsWithAlbumAndNumber_ReturnsTrue()
    {
        var song = CreateSong("Best Album 05 Song Title", 5, "Best Album");

        var result = song.TitleHasUnwantedText();

        Assert.True(result);
    }

    [Fact]
    public void TitleHasUnwantedText_TitleStartsWithAlbumPartAndNumber_DocumentsBehavior()
    {
        // This documents that partial album matches are not detected
        var song = CreateSong("Best 03 Song Title", 3, "Best Album");

        var result = song.TitleHasUnwantedText();

        Assert.False(result); // Actual behavior: only full album name prefix is matched
    }

    [Fact]
    public void TitleHasUnwantedText_ValidTitle_ReturnsFalse()
    {
        var song = CreateSong("Great Song");

        var result = song.TitleHasUnwantedText();

        Assert.False(result);
    }

    [Fact]
    public void TitleHasUnwantedText_ValidTitleWithSingleSpace_ReturnsFalse()
    {
        var song = CreateSong("Great Song Title");

        var result = song.TitleHasUnwantedText();

        Assert.False(result);
    }

    [Fact]
    public void TitleHasUnwantedText_TitleWithDigitsButNotPrefix_ReturnsFalse()
    {
        var song = CreateSong("Track5", 1);

        var result = song.TitleHasUnwantedText();

        Assert.False(result);
    }

    [Fact]
    public void TitleHasUnwantedText_TitleWithDifferentNumberThanSongNumber_ReturnsFalse()
    {
        var song = CreateSong("Track 99", 1);

        var result = song.TitleHasUnwantedText();

        Assert.False(result);
    }

    [Fact]
    public void TitleHasUnwantedText_TitleWithYearInMiddle_ReturnsFalse()
    {
        var song = CreateSong("Song 1999 Remix", 3);

        var result = song.TitleHasUnwantedText();

        Assert.False(result);
    }

    [Theory]
    [InlineData("B2B")]
    [InlineData("2Pac")]
    [InlineData("3rd Strike")]
    public void TitleHasUnwantedText_ArtistNamesWithNumbers_ReturnsFalse(string title)
    {
        var song = CreateSong(title, 1);

        var result = song.TitleHasUnwantedText();

        Assert.False(result);
    }

    [Fact]
    public void TitleHasUnwantedText_ExceptionThrown_ReturnsFalse()
    {
        var song = new Song
        {
            Tags = new[]
            {
                new MetaTag<object?> { Identifier = MetaTagIdentifier.Title, Value = "Test" },
                new MetaTag<object?> { Identifier = MetaTagIdentifier.Album, Value = "Album[" }
            },
            File = new FileSystemFileInfo { Name = "test.mp3", Size = 1000 },
            CrcHash = string.Empty
        };

        var result = song.TitleHasUnwantedText();

        Assert.False(result);
    }
}
