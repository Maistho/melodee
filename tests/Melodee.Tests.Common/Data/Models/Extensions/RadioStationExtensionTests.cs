using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;

namespace Melodee.Tests.Common.Data.Models.Extensions;

public class RadioStationExtensionTests
{
    [Fact]
    public void ToApiKey_ShouldReturnRadioStationApiKeyWithPrefix()
    {
        // Arrange
        var radioStation = new RadioStation
        {
            ApiKey = Guid.NewGuid(),
            Name = "Test Radio Station",
            StreamUrl = "http://example.com/stream",
            HomePageUrl = "http://example.com",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        // Act
        var result = radioStation.ToApiKey();

        // Assert
        Assert.StartsWith("radio_", result);
        Assert.Contains(radioStation.ApiKey.ToString(), result);
    }

    [Fact]
    public void ToApiInternetRadioStation_ShouldReturnValidApiInternetRadioStation()
    {
        // Arrange
        var radioStation = new RadioStation
        {
            ApiKey = Guid.NewGuid(),
            Name = "Test Radio Station",
            StreamUrl = "http://example.com/stream",
            HomePageUrl = "http://example.com",
            CreatedAt = NodaTime.Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)
        };

        // Act
        var result = radioStation.ToApiInternetRadioStation();

        // Assert
        Assert.Contains(radioStation.ApiKey.ToString(), result.Id);
        Assert.Equal(radioStation.Name, result.Name);
        Assert.Equal(radioStation.StreamUrl, result.StreamUrl);
        Assert.Equal(radioStation.HomePageUrl, result.HomePageUrl);
    }
}
