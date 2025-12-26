using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Tests.Common.Common.Services;

public class RequestAutoCompletionServiceTests : ServiceTestBase
{
    private RequestAutoCompletionService GetAutoCompletionService()
    {
        return new RequestAutoCompletionService(Logger, CacheManager, MockFactory());
    }

    private RequestService GetRequestService()
    {
        return new RequestService(Logger, CacheManager, MockFactory());
    }

    private async Task<User> CreateTestUserAsync()
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var user = new User
        {
            UserName = $"testuser_{uniqueId}",
            UserNameNormalized = $"testuser_{uniqueId}",
            Email = $"test_{uniqueId}@example.com",
            EmailNormalized = $"test_{uniqueId}@example.com",
            PublicKey = "publickey",
            PasswordEncrypted = "password",
            IsAdmin = false,
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<(Artist artist, Album album)> CreateTestArtistAndAlbumAsync(string artistName, string albumName, int? releaseYear = null)
    {
        await using var context = await MockFactory().CreateDbContextAsync();

        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library",
            Type = (int)LibraryType.Storage,
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.Libraries.Add(library);
        await context.SaveChangesAsync();

        var artist = new Artist
        {
            Name = artistName,
            NameNormalized = artistName.ToLowerInvariant(),
            SortName = artistName,
            Directory = artistName,
            ApiKey = Guid.NewGuid(),
            LibraryId = library.Id,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        var album = new Album
        {
            Name = albumName,
            NameNormalized = albumName.ToLowerInvariant(),
            SortName = albumName,
            Directory = albumName,
            ApiKey = Guid.NewGuid(),
            ArtistId = artist.Id,
            ReleaseDate = new LocalDate(releaseYear ?? 2024, 1, 1),
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
        context.Albums.Add(album);
        await context.SaveChangesAsync();

        return (artist, album);
    }

    [Fact]
    public async Task ProcessAlbumAddedAsync_CompletesMatchingRequest_ByArtistAndAlbumName()
    {
        var user = await CreateTestUserAsync();
        var requestService = GetRequestService();
        var autoCompletionService = GetAutoCompletionService();

        var request = new Request
        {
            Category = (int)RequestCategory.AddAlbum,
            Description = "Please add this album",
            ArtistName = "Test Artist",
            AlbumTitle = "Test Album"
        };
        var createResult = await requestService.CreateAsync(request, user.Id);
        Assert.True(createResult.IsSuccess);

        var (artist, album) = await CreateTestArtistAndAlbumAsync("Test Artist", "Test Album");

        var completedCount = await autoCompletionService.ProcessAlbumAddedAsync(album);

        Assert.Equal(1, completedCount);

        var getResult = await requestService.GetByApiKeyAsync(createResult.Data!.ApiKey);
        Assert.Equal(RequestStatus.Completed, getResult.Data!.StatusValue);
        Assert.Equal(RequestActivityType.SystemComment, getResult.Data.LastActivityTypeValue);
    }

    [Fact]
    public async Task ProcessAlbumAddedAsync_CompletesMatchingRequest_ByTargetArtistApiKey()
    {
        var user = await CreateTestUserAsync();
        var requestService = GetRequestService();
        var autoCompletionService = GetAutoCompletionService();

        var (artist, album) = await CreateTestArtistAndAlbumAsync("Some Artist", "Some Album");

        var request = new Request
        {
            Category = (int)RequestCategory.AddAlbum,
            Description = "Please add this album",
            TargetArtistApiKey = artist.ApiKey
        };
        var createResult = await requestService.CreateAsync(request, user.Id);
        Assert.True(createResult.IsSuccess);

        var completedCount = await autoCompletionService.ProcessAlbumAddedAsync(album);

        Assert.Equal(1, completedCount);

        var getResult = await requestService.GetByApiKeyAsync(createResult.Data!.ApiKey);
        Assert.Equal(RequestStatus.Completed, getResult.Data!.StatusValue);
    }

    [Fact]
    public async Task ProcessAlbumAddedAsync_DoesNotComplete_WhenReleaseYearMismatch()
    {
        var user = await CreateTestUserAsync();
        var requestService = GetRequestService();
        var autoCompletionService = GetAutoCompletionService();

        var request = new Request
        {
            Category = (int)RequestCategory.AddAlbum,
            Description = "Please add this album",
            ArtistName = "Test Artist",
            AlbumTitle = "Test Album",
            ReleaseYear = 2020
        };
        var createResult = await requestService.CreateAsync(request, user.Id);
        Assert.True(createResult.IsSuccess);

        var (artist, album) = await CreateTestArtistAndAlbumAsync("Test Artist", "Test Album", 2024);

        var completedCount = await autoCompletionService.ProcessAlbumAddedAsync(album);

        Assert.Equal(0, completedCount);

        var getResult = await requestService.GetByApiKeyAsync(createResult.Data!.ApiKey);
        Assert.Equal(RequestStatus.Pending, getResult.Data!.StatusValue);
    }

    [Fact]
    public async Task ProcessAlbumAddedAsync_DoesNotComplete_AlreadyCompletedRequest()
    {
        var user = await CreateTestUserAsync();
        var requestService = GetRequestService();
        var autoCompletionService = GetAutoCompletionService();

        var request = new Request
        {
            Category = (int)RequestCategory.AddAlbum,
            Description = "Please add this album",
            ArtistName = "Test Artist",
            AlbumTitle = "Test Album"
        };
        var createResult = await requestService.CreateAsync(request, user.Id);
        await requestService.CompleteAsync(createResult.Data!.ApiKey, user.Id);

        var (artist, album) = await CreateTestArtistAndAlbumAsync("Test Artist", "Test Album");

        var completedCount = await autoCompletionService.ProcessAlbumAddedAsync(album);

        Assert.Equal(0, completedCount);
    }

    [Fact]
    public async Task ProcessAlbumAddedAsync_DoesNotComplete_WrongCategory()
    {
        var user = await CreateTestUserAsync();
        var requestService = GetRequestService();
        var autoCompletionService = GetAutoCompletionService();

        var request = new Request
        {
            Category = (int)RequestCategory.AddSong,
            Description = "Please add this song",
            ArtistName = "Test Artist",
            SongTitle = "Test Song"
        };
        var createResult = await requestService.CreateAsync(request, user.Id);
        Assert.True(createResult.IsSuccess);

        var (artist, album) = await CreateTestArtistAndAlbumAsync("Test Artist", "Test Album");

        var completedCount = await autoCompletionService.ProcessAlbumAddedAsync(album);

        Assert.Equal(0, completedCount);

        var getResult = await requestService.GetByApiKeyAsync(createResult.Data!.ApiKey);
        Assert.Equal(RequestStatus.Pending, getResult.Data!.StatusValue);
    }

    [Fact]
    public async Task ProcessAlbumAddedAsync_CreatesSystemComment_OnCompletion()
    {
        var user = await CreateTestUserAsync();
        var requestService = GetRequestService();
        var autoCompletionService = GetAutoCompletionService();

        var request = new Request
        {
            Category = (int)RequestCategory.AddAlbum,
            Description = "Please add this album",
            ArtistName = "Test Artist",
            AlbumTitle = "Test Album"
        };
        var createResult = await requestService.CreateAsync(request, user.Id);

        var (artist, album) = await CreateTestArtistAndAlbumAsync("Test Artist", "Test Album");

        await autoCompletionService.ProcessAlbumAddedAsync(album);

        await using var context = await MockFactory().CreateDbContextAsync();
        var comments = await context.RequestComments
            .Where(c => c.RequestId == createResult.Data!.Id)
            .ToListAsync();

        Assert.Single(comments);
        Assert.True(comments.First().IsSystem);
        Assert.Contains("automatically completed", comments.First().Body);
        Assert.Contains(album.ApiKey.ToString(), comments.First().Body);
    }

    [Fact]
    public async Task ProcessAlbumAddedAsync_ReturnsZero_WhenNoOpenRequests()
    {
        var autoCompletionService = GetAutoCompletionService();
        var (artist, album) = await CreateTestArtistAndAlbumAsync("Test Artist", "Test Album");

        var completedCount = await autoCompletionService.ProcessAlbumAddedAsync(album);

        Assert.Equal(0, completedCount);
    }
}
