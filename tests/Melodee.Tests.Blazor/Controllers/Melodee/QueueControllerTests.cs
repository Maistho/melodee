using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for QueueController endpoints.
/// </summary>
public class QueueControllerTests
{
    #region Get Queue Tests

    [Fact]
    public void GetQueueAsync_HasHttpGetAttribute()
    {
        // Arrange
        var method = typeof(QueueController).GetMethod(nameof(QueueController.GetQueueAsync));

        // Assert
        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetQueueAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(QueueController).GetMethod(nameof(QueueController.GetQueueAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].Name.Should().Be("cancellationToken");
        parameters[0].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void GetQueueAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(QueueController).GetMethod(nameof(QueueController.GetQueueAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Save Queue Tests

    [Fact]
    public void SaveQueueAsync_HasHttpPutAttribute()
    {
        // Arrange
        var method = typeof(QueueController).GetMethod(nameof(QueueController.SaveQueueAsync));

        // Assert
        method.Should().NotBeNull();
        var httpPutAttribute = method!.GetCustomAttributes(typeof(HttpPutAttribute), false).FirstOrDefault();
        httpPutAttribute.Should().NotBeNull();
    }

    [Fact]
    public void SaveQueueAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(QueueController).GetMethod(nameof(QueueController.SaveQueueAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be("request");
        parameters[0].ParameterType.Should().Be(typeof(SaveQueueRequest));
        parameters[1].Name.Should().Be("cancellationToken");
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void SaveQueueAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(QueueController).GetMethod(nameof(QueueController.SaveQueueAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Clear Queue Tests

    [Fact]
    public void ClearQueueAsync_HasHttpDeleteAttribute()
    {
        // Arrange
        var method = typeof(QueueController).GetMethod(nameof(QueueController.ClearQueueAsync));

        // Assert
        method.Should().NotBeNull();
        var httpDeleteAttribute = method!.GetCustomAttributes(typeof(HttpDeleteAttribute), false).FirstOrDefault();
        httpDeleteAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ClearQueueAsync_HasCorrectParameters()
    {
        // Arrange
        var method = typeof(QueueController).GetMethod(nameof(QueueController.ClearQueueAsync));

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].Name.Should().Be("cancellationToken");
        parameters[0].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void ClearQueueAsync_ReturnsTaskOfIActionResult()
    {
        // Arrange
        var method = typeof(QueueController).GetMethod(nameof(QueueController.ClearQueueAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region Request Model Tests

    [Fact]
    public void SaveQueueRequest_HasAllRequiredProperties()
    {
        // Arrange & Act
        var songIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var currentSongId = songIds[0];
        var request = new SaveQueueRequest(songIds, currentSongId, 30.5, "TestClient");

        // Assert
        request.SongIds.Should().HaveCount(2);
        request.CurrentSongId.Should().Be(currentSongId);
        request.Position.Should().Be(30.5);
        request.ChangedBy.Should().Be("TestClient");
    }

    [Fact]
    public void SaveQueueRequest_WithNullValues_CanBeCreated()
    {
        // Arrange & Act
        var request = new SaveQueueRequest(null, null, null, null);

        // Assert
        request.SongIds.Should().BeNull();
        request.CurrentSongId.Should().BeNull();
        request.Position.Should().BeNull();
        request.ChangedBy.Should().BeNull();
    }

    [Fact]
    public void SaveQueueRequest_WithEmptyArray_CanBeCreated()
    {
        // Arrange & Act
        var request = new SaveQueueRequest([], null, null, null);

        // Assert
        request.SongIds.Should().BeEmpty();
    }

    #endregion
}
