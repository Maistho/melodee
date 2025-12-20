using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee.Models;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

public class ScrobbleRequestTests
{
    #region Constructor Tests

    [Fact]
    public void ScrobbleRequest_WithAllParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var songId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var playedDuration = 180.5;

        // Act
        var request = new ScrobbleRequest(songId, "TestPlayer", "Played", timestamp, playedDuration);

        // Assert
        request.SongId.Should().Be(songId);
        request.PlayerName.Should().Be("TestPlayer");
        request.ScrobbleType.Should().Be("Played");
        request.Timestamp.Should().Be(timestamp);
        request.PlayedDuration.Should().Be(playedDuration);
    }

    [Fact]
    public void ScrobbleRequest_WithNullOptionalParameters_HasNullValues()
    {
        // Arrange
        var songId = Guid.NewGuid();

        // Act
        var request = new ScrobbleRequest(songId, "Player", "NowPlaying", null, null);

        // Assert
        request.Timestamp.Should().BeNull();
        request.PlayedDuration.Should().BeNull();
    }

    #endregion

    #region ScrobbleTypeValue Tests

    [Theory]
    [InlineData("NowPlaying", ScrobbleRequestType.NowPlaying)]
    [InlineData("Played", ScrobbleRequestType.Played)]
    [InlineData("nowplaying", ScrobbleRequestType.NowPlaying)]
    [InlineData("played", ScrobbleRequestType.Played)]
    [InlineData("NOWPLAYING", ScrobbleRequestType.NowPlaying)]
    [InlineData("PLAYED", ScrobbleRequestType.Played)]
    public void ScrobbleTypeValue_ValidType_ReturnsCorrectEnum(string scrobbleType, ScrobbleRequestType expected)
    {
        // Arrange
        var request = new ScrobbleRequest(Guid.NewGuid(), "Player", scrobbleType, null, null);

        // Act & Assert
        request.ScrobbleTypeValue.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidType")]
    [InlineData("Unknown")]
    public void ScrobbleTypeValue_InvalidType_ReturnsNotSet(string scrobbleType)
    {
        // Arrange
        var request = new ScrobbleRequest(Guid.NewGuid(), "Player", scrobbleType, null, null);

        // Act & Assert
        request.ScrobbleTypeValue.Should().Be(ScrobbleRequestType.NotSet);
    }

    [Fact]
    public void ScrobbleTypeValue_NumericString_ParsesAsEnumValue()
    {
        // Arrange - The SafeParser may parse numeric strings as enum values
        var request = new ScrobbleRequest(Guid.NewGuid(), "Player", "123", null, null);

        // Act & Assert - Numeric string "123" gets parsed to enum value 123
        // This is expected behavior of SafeParser.ToEnum
        ((int)request.ScrobbleTypeValue).Should().Be(123);
    }

    [Fact]
    public void ScrobbleTypeValue_NullType_ReturnsNotSet()
    {
        // Arrange - Using null! to bypass nullable warning for testing
        var request = new ScrobbleRequest(Guid.NewGuid(), "Player", null!, null, null);

        // Act & Assert
        request.ScrobbleTypeValue.Should().Be(ScrobbleRequestType.NotSet);
    }

    #endregion

    #region ScrobbleRequestType Enum Tests

    [Fact]
    public void ScrobbleRequestType_NotSet_HasValueZero()
    {
        // Assert
        ((int)ScrobbleRequestType.NotSet).Should().Be(0);
    }

    [Fact]
    public void ScrobbleRequestType_AllValuesAreDefined()
    {
        // Assert
        Enum.GetValues<ScrobbleRequestType>().Should().Contain(ScrobbleRequestType.NotSet);
        Enum.GetValues<ScrobbleRequestType>().Should().Contain(ScrobbleRequestType.NowPlaying);
        Enum.GetValues<ScrobbleRequestType>().Should().Contain(ScrobbleRequestType.Played);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void ScrobbleRequest_Equality_WorksCorrectly()
    {
        // Arrange
        var songId = Guid.NewGuid();
        var request1 = new ScrobbleRequest(songId, "Player", "Played", 123.0, 180.0);
        var request2 = new ScrobbleRequest(songId, "Player", "Played", 123.0, 180.0);
        var request3 = new ScrobbleRequest(Guid.NewGuid(), "Player", "Played", 123.0, 180.0);

        // Assert
        request1.Should().Be(request2);
        request1.Should().NotBe(request3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ScrobbleRequest_EmptySongId_IsAccepted()
    {
        // Arrange & Act
        var request = new ScrobbleRequest(Guid.Empty, "Player", "Played", null, null);

        // Assert
        request.SongId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ScrobbleRequest_EmptyPlayerName_IsAccepted()
    {
        // Arrange & Act
        var request = new ScrobbleRequest(Guid.NewGuid(), "", "Played", null, null);

        // Assert
        request.PlayerName.Should().BeEmpty();
    }

    [Fact]
    public void ScrobbleRequest_NegativePlayedDuration_IsAccepted()
    {
        // Arrange & Act - Model accepts invalid values; validation should be done elsewhere
        var request = new ScrobbleRequest(Guid.NewGuid(), "Player", "Played", null, -100.0);

        // Assert
        request.PlayedDuration.Should().Be(-100.0);
    }

    [Fact]
    public void ScrobbleRequest_ZeroPlayedDuration_IsAccepted()
    {
        // Arrange & Act
        var request = new ScrobbleRequest(Guid.NewGuid(), "Player", "Played", null, 0.0);

        // Assert
        request.PlayedDuration.Should().Be(0.0);
    }

    #endregion
}
