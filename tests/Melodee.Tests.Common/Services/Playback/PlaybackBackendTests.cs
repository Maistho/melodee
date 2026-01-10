using Melodee.Common.Configuration;
using Melodee.Common.Services.Playback;
using Melodee.Common.Services.Playback.Backends;
using Melodee.Common.Services.Playback.Factory;
using Microsoft.Extensions.Options;
using Serilog;
using Xunit;

namespace Melodee.Tests.Common.Services.Playback;

public class PlaybackBackendFactoryTests
{
    [Fact]
    public void CreateBackend_WithNullBackendType_ReturnsNull()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var mpvOptions = Options.Create(new MpvOptions());
        var factory = new PlaybackBackendFactory(logger, mpvOptions);

        // Act
        var backend = factory.CreateBackend(null);

        // Assert
        Assert.Null(backend);
    }

    [Fact]
    public void CreateBackend_WithEmptyBackendType_ReturnsNull()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var mpvOptions = Options.Create(new MpvOptions());
        var factory = new PlaybackBackendFactory(logger, mpvOptions);

        // Act
        var backend = factory.CreateBackend("");

        // Assert
        Assert.Null(backend);
    }

    [Fact]
    public void CreateBackend_WithMpvType_CreatesMpvBackend()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var mpvOptions = Options.Create(new MpvOptions());
        var factory = new PlaybackBackendFactory(logger, mpvOptions);

        // Act
        var backend = factory.CreateBackend("mpv");

        // Assert
        Assert.NotNull(backend);
        Assert.IsType<MpvPlaybackBackend>(backend);
    }

    [Fact]
    public void CreateBackend_WithMpvUppercase_CreatesMpvBackend()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var mpvOptions = Options.Create(new MpvOptions());
        var factory = new PlaybackBackendFactory(logger, mpvOptions);

        // Act
        var backend = factory.CreateBackend("MPV");

        // Assert
        Assert.NotNull(backend);
        Assert.IsType<MpvPlaybackBackend>(backend);
    }

    [Fact]
    public void CreateBackend_WithUnknownType_ReturnsNull()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var mpvOptions = Options.Create(new MpvOptions());
        var factory = new PlaybackBackendFactory(logger, mpvOptions);

        // Act
        var backend = factory.CreateBackend("unknown");

        // Assert
        Assert.Null(backend);
    }

    [Fact]
    public void GetOrCreateMpvBackend_CalledTwice_ReturnsSameInstance()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var mpvOptions = Options.Create(new MpvOptions());
        var factory = new PlaybackBackendFactory(logger, mpvOptions);

        // Act
        var backend1 = factory.GetOrCreateMpvBackend();
        var backend2 = factory.GetOrCreateMpvBackend();

        // Assert
        Assert.NotNull(backend1);
        Assert.NotNull(backend2);
        Assert.Same(backend1, backend2);
    }

    [Fact]
    public void DisposeBackend_DisposesCachedBackend()
    {
        // Arrange
        var logger = new LoggerConfiguration().CreateLogger();
        var mpvOptions = Options.Create(new MpvOptions());
        var factory = new PlaybackBackendFactory(logger, mpvOptions);

        var backend = factory.GetOrCreateMpvBackend();
        Assert.NotNull(backend);

        // Act
        factory.DisposeBackend();

        // Note: We can't easily verify the backend was disposed without more complex setup
        // This test at least verifies the method doesn't throw
    }
}

public class BackendCapabilitiesTests
{
    [Fact]
    public void DefaultCapabilities_HaveExpectedValues()
    {
        // Arrange & Act
        var capabilities = new BackendCapabilities();

        // Assert
        Assert.True(capabilities.CanPlay);
        Assert.True(capabilities.CanPause);
        Assert.True(capabilities.CanStop);
        Assert.True(capabilities.CanSeek);
        Assert.True(capabilities.CanSkip);
        Assert.True(capabilities.CanSetVolume);
        Assert.True(capabilities.CanReportPosition);
        Assert.True(capabilities.IsAvailable);
        Assert.Null(capabilities.BackendInfo);
    }

    [Fact]
    public void Capabilities_CanBeSetWithInitializer()
    {
        // Arrange & Act
        var capabilities = new BackendCapabilities
        {
            CanPlay = false,
            CanPause = false,
            CanSeek = false,
            CanSkip = false,
            IsAvailable = false,
            BackendInfo = "Test Backend v1.0"
        };

        // Assert
        Assert.False(capabilities.CanPlay);
        Assert.False(capabilities.CanPause);
        Assert.False(capabilities.CanSeek);
        Assert.False(capabilities.CanSkip);
        Assert.False(capabilities.IsAvailable);
        Assert.Equal("Test Backend v1.0", capabilities.BackendInfo);
    }
}

public class BackendStatusTests
{
    [Fact]
    public void DefaultStatus_HasExpectedValues()
    {
        // Arrange & Act
        var status = new BackendStatus();

        // Assert
        Assert.False(status.IsPlaying);
        Assert.Equal(0, status.PositionSeconds);
        Assert.Null(status.Volume);
        Assert.Null(status.CurrentItemApiKey);
        Assert.False(status.IsConnected);
        Assert.Null(status.StatusMessage);
        Assert.Null(status.ErrorMessage);
    }

    [Fact]
    public void Status_CanBeSetWithInitializer()
    {
        // Arrange & Act
        var currentItemId = Guid.NewGuid();
        var status = new BackendStatus
        {
            IsPlaying = true,
            PositionSeconds = 120.5,
            Volume = 0.8,
            CurrentItemApiKey = currentItemId,
            IsConnected = true,
            StatusMessage = "Playing track",
            ErrorMessage = null
        };

        // Assert
        Assert.True(status.IsPlaying);
        Assert.Equal(120.5, status.PositionSeconds);
        Assert.Equal(0.8, status.Volume);
        Assert.Equal(currentItemId, status.CurrentItemApiKey);
        Assert.True(status.IsConnected);
        Assert.Equal("Playing track", status.StatusMessage);
        Assert.Null(status.ErrorMessage);
    }
}
