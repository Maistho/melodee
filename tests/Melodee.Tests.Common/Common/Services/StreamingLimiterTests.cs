using Melodee.Common.Configuration;
using Melodee.Common.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Melodee.Tests.Common.Common.Services;

public class StreamingLimiterTests
{
    [Fact]
    public void Constructor_InitializesSuccessfully()
    {
        // Arrange
        var mockConfigurationFactory = new Mock<IMelodeeConfigurationFactory>();

        // Act
        var service = new StreamingLimiter(mockConfigurationFactory.Object);

        // Assert
        Assert.NotNull(service);
    }
}
