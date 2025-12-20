using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee.Models;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

public class SearchResultTests
{
    #region Constructor Tests

    [Fact]
    public void SearchResult_WithEmptyArrays_SetsAllPropertiesCorrectly()
    {
        // Arrange & Act
        var result = new SearchResult(0, [], 0, [], 0, [], 0, [], 0);

        // Assert
        result.TotalCount.Should().Be(0);
        result.Artists.Should().BeEmpty();
        result.TotalArtists.Should().Be(0);
        result.Albums.Should().BeEmpty();
        result.TotalAlbums.Should().Be(0);
        result.Songs.Should().BeEmpty();
        result.TotalSongs.Should().Be(0);
        result.Playlists.Should().BeEmpty();
        result.TotalPlaylists.Should().Be(0);
    }

    [Fact]
    public void SearchResult_WithData_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var artistId = Guid.NewGuid();
        var artists = new[]
        {
            new Artist(artistId, "/thumb", "/img", "Artist 1", false, 0, 10, 100, "2023-01-01", "2023-01-01")
        };
        var albums = new[]
        {
            new Album(Guid.NewGuid(), Artist.BlankArtist(), "/thumb", "/img", "Album 1", 2020, false, 0, 10, 2400000, "40:00", "2023-01-01", "2023-01-01")
        };
        var songs = Array.Empty<Song>();
        var playlists = Array.Empty<Playlist>();

        // Act
        var result = new SearchResult(15, artists, 5, albums, 8, songs, 2, playlists, 0);

        // Assert
        result.TotalCount.Should().Be(15);
        result.Artists.Should().HaveCount(1);
        result.TotalArtists.Should().Be(5);
        result.Albums.Should().HaveCount(1);
        result.TotalAlbums.Should().Be(8);
        result.Songs.Should().BeEmpty();
        result.TotalSongs.Should().Be(2);
        result.Playlists.Should().BeEmpty();
        result.TotalPlaylists.Should().Be(0);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void SearchResult_EmptySearch_HasZeroTotals()
    {
        // Arrange & Act
        var result = new SearchResult(0, [], 0, [], 0, [], 0, [], 0);

        // Assert
        result.TotalCount.Should().Be(0);
        (result.TotalArtists + result.TotalAlbums + result.TotalSongs + result.TotalPlaylists).Should().Be(0);
    }

    [Fact]
    public void SearchResult_TotalCount_CanBeLessThanSumOfTotals()
    {
        // This tests when pagination limits are applied
        // TotalCount might reflect the combined result, while individual totals are for each type
        var result = new SearchResult(50, [], 100, [], 200, [], 300, [], 50);

        result.TotalCount.Should().Be(50);
        // Individual totals can exceed TotalCount when they represent total available, not returned
        (result.TotalArtists + result.TotalAlbums + result.TotalSongs + result.TotalPlaylists).Should().Be(650);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void SearchResult_Equality_WorksCorrectly()
    {
        // Arrange
        var result1 = new SearchResult(10, [], 5, [], 5, [], 0, [], 0);
        var result2 = new SearchResult(10, [], 5, [], 5, [], 0, [], 0);
        var result3 = new SearchResult(20, [], 10, [], 10, [], 0, [], 0);

        // Assert
        result1.Should().Be(result2);
        result1.Should().NotBe(result3);
    }

    #endregion
}
