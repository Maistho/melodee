using Melodee.Common.Services.Extensions;
using NodaTime;

namespace Melodee.Tests.Common.Services.Extensions;

public class LocalDateExtensionTests
{
    [Fact]
    public void ToItemDate_ShouldReturnValidItemDate()
    {
        // Arrange
        var localDate = new LocalDate(2023, 5, 15);

        // Act
        var result = localDate.ToItemDate();

        // Assert
        Assert.Equal(2023, result.Year);
        Assert.Equal(5, result.Month);
        Assert.Equal(15, result.Day);
    }

    [Fact]
    public void ToItemDate_WithDifferentDate_ShouldReturnCorrectValues()
    {
        // Arrange
        var localDate = new LocalDate(1999, 12, 31);

        // Act
        var result = localDate.ToItemDate();

        // Assert
        Assert.Equal(1999, result.Year);
        Assert.Equal(12, result.Month);
        Assert.Equal(31, result.Day);
    }

    [Fact]
    public void ToItemDate_WithLeapYearDate_ShouldReturnCorrectValues()
    {
        // Arrange
        var localDate = new LocalDate(2024, 2, 29); // Leap year

        // Act
        var result = localDate.ToItemDate();

        // Assert
        Assert.Equal(2024, result.Year);
        Assert.Equal(2, result.Month);
        Assert.Equal(29, result.Day);
    }
}
