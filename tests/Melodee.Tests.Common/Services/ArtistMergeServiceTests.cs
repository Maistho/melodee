using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Tests.Common.Services;

/// <summary>
/// Comprehensive tests for the ArtistService.MergeArtistsAsync functionality.
/// These tests validate that all related data elements are properly transferred
/// during artist merge operations.
/// </summary>
public class ArtistMergeServiceTests : ServiceTestBase
{
    #region Basic Merge Tests

    [Fact]
    public async Task MergeArtistsAsync_SingleSourceArtist_MergesSuccessfully()
    {
        var (targetArtist, sourceArtist, _) = await CreateMergeTestArtists();

        var result = await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        await using var context = await MockFactory().CreateDbContextAsync();
        var deletedSource = await context.Artists.FindAsync(sourceArtist.Id);
        Assert.Null(deletedSource);
    }

    [Fact]
    public async Task MergeArtistsAsync_MultipleSourceArtists_MergesAllSuccessfully()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist1 = await CreateArtistInLibrary(library, "Source Artist 1");
        var sourceArtist2 = await CreateArtistInLibrary(library, "Source Artist 2");
        var sourceArtist3 = await CreateArtistInLibrary(library, "Source Artist 3");

        var result = await GetArtistService().MergeArtistsAsync(
            targetArtist.Id,
            [sourceArtist1.Id, sourceArtist2.Id, sourceArtist3.Id]);

        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        await using var context = await MockFactory().CreateDbContextAsync();
        Assert.Null(await context.Artists.FindAsync(sourceArtist1.Id));
        Assert.Null(await context.Artists.FindAsync(sourceArtist2.Id));
        Assert.Null(await context.Artists.FindAsync(sourceArtist3.Id));
        Assert.NotNull(await context.Artists.FindAsync(targetArtist.Id));
    }

    #endregion

    #region Alternate Names Tests

    [Fact]
    public async Task MergeArtistsAsync_TransfersSourceArtistNameAsAlternateName()
    {
        var (targetArtist, sourceArtist, _) = await CreateMergeTestArtists();

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var updatedTarget = await context.Artists.FindAsync(targetArtist.Id);
        Assert.NotNull(updatedTarget);
        Assert.Contains(sourceArtist.NameNormalized, updatedTarget.AlternateNames ?? string.Empty);
    }

    [Fact]
    public async Task MergeArtistsAsync_TransfersSourceAlternateNames()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var dbSource = await context.Artists.FindAsync(sourceArtist.Id);
            dbSource!.AlternateNames = "ALIASONE|ALIASTWO|ALIASTHREE";
            await context.SaveChangesAsync();
        }

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var updatedTarget = await context.Artists.FindAsync(targetArtist.Id);
            Assert.NotNull(updatedTarget);
            Assert.Contains("ALIASONE", updatedTarget.AlternateNames ?? string.Empty);
            Assert.Contains("ALIASTWO", updatedTarget.AlternateNames ?? string.Empty);
            Assert.Contains("ALIASTHREE", updatedTarget.AlternateNames ?? string.Empty);
        }
    }

    [Fact]
    public async Task MergeArtistsAsync_PreservesTargetExistingAlternateNames()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var dbTarget = await context.Artists.FindAsync(targetArtist.Id);
            dbTarget!.AlternateNames = "EXISTINGALIAS|ORIGINALNAME";
            await context.SaveChangesAsync();
        }

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var updatedTarget = await context.Artists.FindAsync(targetArtist.Id);
            Assert.NotNull(updatedTarget);
            Assert.Contains("EXISTINGALIAS", updatedTarget.AlternateNames ?? string.Empty);
            Assert.Contains("ORIGINALNAME", updatedTarget.AlternateNames ?? string.Empty);
        }
    }

    [Fact]
    public async Task MergeArtistsAsync_DeduplicatesAlternateNames()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist1 = await CreateArtistInLibrary(library, "Source Artist 1");
        var sourceArtist2 = await CreateArtistInLibrary(library, "Source Artist 2");

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var dbSource1 = await context.Artists.FindAsync(sourceArtist1.Id);
            var dbSource2 = await context.Artists.FindAsync(sourceArtist2.Id);
            dbSource1!.AlternateNames = "COMMONALIAS|UNIQUEONE";
            dbSource2!.AlternateNames = "COMMONALIAS|UNIQUETWO";
            await context.SaveChangesAsync();
        }

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist1.Id, sourceArtist2.Id]);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var updatedTarget = await context.Artists.FindAsync(targetArtist.Id);
            Assert.NotNull(updatedTarget);
            var aliases = updatedTarget.AlternateNames?.Split('|') ?? [];
            var commonAliasCount = aliases.Count(a => a.Equals("COMMONALIAS", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(1, commonAliasCount);
        }
    }

    #endregion

    #region Albums Transfer Tests

    [Fact]
    public async Task MergeArtistsAsync_TransfersAlbumsToTargetArtist()
    {
        var (targetArtist, sourceArtist, library) = await CreateMergeTestArtists();
        var album = await CreateAlbumForArtist(sourceArtist, "Test Album");

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var transferredAlbum = await context.Albums.FindAsync(album.Id);
        Assert.NotNull(transferredAlbum);
        Assert.Equal(targetArtist.Id, transferredAlbum.ArtistId);
    }

    [Fact]
    public async Task MergeArtistsAsync_TransfersMultipleAlbumsFromMultipleSources()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist1 = await CreateArtistInLibrary(library, "Source Artist 1");
        var sourceArtist2 = await CreateArtistInLibrary(library, "Source Artist 2");

        var album1 = await CreateAlbumForArtist(sourceArtist1, "Album from Source 1");
        var album2 = await CreateAlbumForArtist(sourceArtist1, "Another Album from Source 1");
        var album3 = await CreateAlbumForArtist(sourceArtist2, "Album from Source 2");

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist1.Id, sourceArtist2.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var albums = context.Albums.Where(a => a.ArtistId == targetArtist.Id).ToList();
        Assert.Equal(3, albums.Count);
        Assert.Contains(albums, a => a.Id == album1.Id);
        Assert.Contains(albums, a => a.Id == album2.Id);
        Assert.Contains(albums, a => a.Id == album3.Id);
    }

    [Fact]
    public async Task MergeArtistsAsync_UpdatesAlbumLastUpdatedAt()
    {
        var (targetArtist, sourceArtist, library) = await CreateMergeTestArtists();
        var album = await CreateAlbumForArtist(sourceArtist, "Test Album");
        var originalLastUpdated = album.LastUpdatedAt;

        await Task.Delay(100);
        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var transferredAlbum = await context.Albums.FindAsync(album.Id);
        Assert.NotNull(transferredAlbum);
        Assert.True(transferredAlbum.LastUpdatedAt > originalLastUpdated || transferredAlbum.LastUpdatedAt != null);
    }

    #endregion

    #region UserArtists Transfer Tests

    [Fact]
    public async Task MergeArtistsAsync_TransfersUserArtistsToTarget()
    {
        var (targetArtist, sourceArtist, _) = await CreateMergeTestArtists();
        var user = await CreateTestUser();
        var userArtist = await CreateUserArtist(user, sourceArtist, isStarred: true, rating: 5);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var transferredUserArtist = await context.UserArtists.FindAsync(userArtist.Id);
        Assert.NotNull(transferredUserArtist);
        Assert.Equal(targetArtist.Id, transferredUserArtist.ArtistId);
        Assert.True(transferredUserArtist.IsStarred);
        Assert.Equal(5, transferredUserArtist.Rating);
    }

    [Fact]
    public async Task MergeArtistsAsync_HandlesUserArtistDuplicates_RemovesDuplicate()
    {
        var (targetArtist, sourceArtist, _) = await CreateMergeTestArtists();
        var user = await CreateTestUser();

        await CreateUserArtist(user, targetArtist, isStarred: true, rating: 4);
        var sourceUserArtist = await CreateUserArtist(user, sourceArtist, isStarred: true, rating: 5);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var userArtists = context.UserArtists.Where(ua => ua.UserId == user.Id && ua.ArtistId == targetArtist.Id).ToList();
        Assert.Single(userArtists);

        var removedUserArtist = await context.UserArtists.FindAsync(sourceUserArtist.Id);
        Assert.Null(removedUserArtist);
    }

    [Fact]
    public async Task MergeArtistsAsync_TransfersUserArtistsFromMultipleUsers()
    {
        var (targetArtist, sourceArtist, _) = await CreateMergeTestArtists();
        var user1 = await CreateTestUser("user1@test.com");
        var user2 = await CreateTestUser("user2@test.com");

        var userArtist1 = await CreateUserArtist(user1, sourceArtist, isStarred: true, rating: 5);
        var userArtist2 = await CreateUserArtist(user2, sourceArtist, isStarred: false, rating: 3);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var transferredUA1 = await context.UserArtists.FindAsync(userArtist1.Id);
        var transferredUA2 = await context.UserArtists.FindAsync(userArtist2.Id);

        Assert.NotNull(transferredUA1);
        Assert.NotNull(transferredUA2);
        Assert.Equal(targetArtist.Id, transferredUA1.ArtistId);
        Assert.Equal(targetArtist.Id, transferredUA2.ArtistId);
    }

    #endregion

    #region UserPins Transfer Tests

    [Fact]
    public async Task MergeArtistsAsync_TransfersUserPinsToTarget()
    {
        var (targetArtist, sourceArtist, _) = await CreateMergeTestArtists();
        var user = await CreateTestUser();
        var userPin = await CreateUserPin(user, sourceArtist);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var transferredPin = await context.UserPins.FindAsync(userPin.Id);
        Assert.NotNull(transferredPin);
        Assert.Equal(targetArtist.Id, transferredPin.PinId);
    }

    [Fact]
    public async Task MergeArtistsAsync_TransfersMultipleUserPins()
    {
        var (targetArtist, sourceArtist, _) = await CreateMergeTestArtists();
        var user1 = await CreateTestUser("user1@test.com");
        var user2 = await CreateTestUser("user2@test.com");

        var pin1 = await CreateUserPin(user1, sourceArtist);
        var pin2 = await CreateUserPin(user2, sourceArtist);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var transferredPin1 = await context.UserPins.FindAsync(pin1.Id);
        var transferredPin2 = await context.UserPins.FindAsync(pin2.Id);

        Assert.NotNull(transferredPin1);
        Assert.NotNull(transferredPin2);
        Assert.Equal(targetArtist.Id, transferredPin1.PinId);
        Assert.Equal(targetArtist.Id, transferredPin2.PinId);
    }

    #endregion

    #region Contributors Transfer Tests

    [Fact]
    public async Task MergeArtistsAsync_TransfersContributorsToTarget()
    {
        var (targetArtist, sourceArtist, library) = await CreateMergeTestArtists();
        var album = await CreateAlbumForArtist(targetArtist, "Album With Contributors");
        var song = await CreateSongForAlbum(album, "Song With Contributor");
        var contributor = await CreateContributor(sourceArtist, album, song, ContributorType.Performer);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var transferredContributor = await context.Contributors.FindAsync(contributor.Id);
        Assert.NotNull(transferredContributor);
        Assert.Equal(targetArtist.Id, transferredContributor.ArtistId);
    }

    [Fact]
    public async Task MergeArtistsAsync_TransfersMultipleContributorsAcrossSongs()
    {
        var (targetArtist, sourceArtist, library) = await CreateMergeTestArtists();
        var album = await CreateAlbumForArtist(targetArtist, "Album With Contributors");
        var song1 = await CreateSongForAlbum(album, "Song 1");
        var song2 = await CreateSongForAlbum(album, "Song 2");

        var contributor1 = await CreateContributor(sourceArtist, album, song1, ContributorType.Performer);
        var contributor2 = await CreateContributor(sourceArtist, album, song1, ContributorType.Production);
        var contributor3 = await CreateContributor(sourceArtist, album, song2, ContributorType.Publisher);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var contributors = context.Contributors.Where(c => c.ArtistId == targetArtist.Id).ToList();
        Assert.Equal(3, contributors.Count);
    }

    #endregion

    #region ArtistRelation Transfer Tests

    [Fact]
    public async Task MergeArtistsAsync_TransfersOutboundArtistRelations()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");
        var relatedArtist = await CreateArtistInLibrary(library, "Related Artist");

        var relation = await CreateArtistRelation(sourceArtist, relatedArtist, ArtistRelationType.Similar);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var transferredRelation = await context.ArtistRelation.FindAsync(relation.Id);
        Assert.NotNull(transferredRelation);
        Assert.Equal(targetArtist.Id, transferredRelation.ArtistId);
        Assert.Equal(relatedArtist.Id, transferredRelation.RelatedArtistId);
    }

    [Fact]
    public async Task MergeArtistsAsync_TransfersInboundArtistRelations()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");
        var otherArtist = await CreateArtistInLibrary(library, "Other Artist");

        var relation = await CreateArtistRelation(otherArtist, sourceArtist, ArtistRelationType.Similar);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var transferredRelation = await context.ArtistRelation.FindAsync(relation.Id);
        Assert.NotNull(transferredRelation);
        Assert.Equal(otherArtist.Id, transferredRelation.ArtistId);
        Assert.Equal(targetArtist.Id, transferredRelation.RelatedArtistId);
    }

    [Fact]
    public async Task MergeArtistsAsync_RemovesDuplicateOutboundRelations()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");
        var relatedArtist = await CreateArtistInLibrary(library, "Related Artist");

        await CreateArtistRelation(targetArtist, relatedArtist, ArtistRelationType.Similar);
        var sourceRelation = await CreateArtistRelation(sourceArtist, relatedArtist, ArtistRelationType.Similar);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var relations = context.ArtistRelation
            .Where(ar => ar.ArtistId == targetArtist.Id && ar.RelatedArtistId == relatedArtist.Id)
            .ToList();
        Assert.Single(relations);

        var removedRelation = await context.ArtistRelation.FindAsync(sourceRelation.Id);
        Assert.Null(removedRelation);
    }

    [Fact]
    public async Task MergeArtistsAsync_RemovesDuplicateInboundRelations()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");
        var otherArtist = await CreateArtistInLibrary(library, "Other Artist");

        await CreateArtistRelation(otherArtist, targetArtist, ArtistRelationType.Similar);
        var sourceRelation = await CreateArtistRelation(otherArtist, sourceArtist, ArtistRelationType.Similar);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var relations = context.ArtistRelation
            .Where(ar => ar.ArtistId == otherArtist.Id && ar.RelatedArtistId == targetArtist.Id)
            .ToList();
        Assert.Single(relations);

        var removedRelation = await context.ArtistRelation.FindAsync(sourceRelation.Id);
        Assert.Null(removedRelation);
    }

    [Fact]
    public async Task MergeArtistsAsync_RemovesSelfReferencingRelations()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");

        var selfRefRelation = await CreateArtistRelation(sourceArtist, targetArtist, ArtistRelationType.Similar);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var removedRelation = await context.ArtistRelation.FindAsync(selfRefRelation.Id);
        Assert.Null(removedRelation);

        var selfRefRelations = context.ArtistRelation
            .Where(ar => ar.ArtistId == targetArtist.Id && ar.RelatedArtistId == targetArtist.Id)
            .ToList();
        Assert.Empty(selfRefRelations);
    }

    [Fact]
    public async Task MergeArtistsAsync_TransfersMultipleRelationTypes()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");
        var relatedArtist1 = await CreateArtistInLibrary(library, "Related Artist 1");
        var relatedArtist2 = await CreateArtistInLibrary(library, "Related Artist 2");

        await CreateArtistRelation(sourceArtist, relatedArtist1, ArtistRelationType.Similar);
        await CreateArtistRelation(sourceArtist, relatedArtist2, ArtistRelationType.Associated);
        await CreateArtistRelation(relatedArtist1, sourceArtist, ArtistRelationType.Similar);

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        await using var context = await MockFactory().CreateDbContextAsync();
        var outboundRelations = context.ArtistRelation.Where(ar => ar.ArtistId == targetArtist.Id).ToList();
        Assert.Equal(2, outboundRelations.Count);

        var inboundRelations = context.ArtistRelation.Where(ar => ar.RelatedArtistId == targetArtist.Id).ToList();
        Assert.Single(inboundRelations);
    }

    #endregion

    #region Aggregate Stats Update Tests

    [Fact]
    public async Task MergeArtistsAsync_UpdatesTargetArtistAlbumCount()
    {
        var (targetArtist, sourceArtist, library) = await CreateMergeTestArtists();

        await CreateAlbumForArtist(targetArtist, "Target Album 1");
        await CreateAlbumForArtist(sourceArtist, "Source Album 1");
        await CreateAlbumForArtist(sourceArtist, "Source Album 2");

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        var updatedArtist = await GetArtistService().GetAsync(targetArtist.Id);
        Assert.NotNull(updatedArtist.Data);
        Assert.Equal(3, updatedArtist.Data.AlbumCount);
    }

    [Fact]
    public async Task MergeArtistsAsync_UpdatesTargetArtistSongCount()
    {
        var (targetArtist, sourceArtist, library) = await CreateMergeTestArtists();

        var targetAlbum = await CreateAlbumForArtist(targetArtist, "Target Album");
        await CreateSongForAlbum(targetAlbum, "Target Song 1");

        var sourceAlbum = await CreateAlbumForArtist(sourceArtist, "Source Album");
        await CreateSongForAlbum(sourceAlbum, "Source Song 1");
        await CreateSongForAlbum(sourceAlbum, "Source Song 2");

        await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        var updatedArtist = await GetArtistService().GetAsync(targetArtist.Id);
        Assert.NotNull(updatedArtist.Data);
        Assert.Equal(3, updatedArtist.Data.SongCount);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task MergeArtistsAsync_NonExistentTarget_ReturnsError()
    {
        var library = await CreateTestLibrary();
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");

        var result = await GetArtistService().MergeArtistsAsync(999999, [sourceArtist.Id]);

        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist to merge into", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    [Fact]
    public async Task MergeArtistsAsync_NonExistentSource_ReturnsError()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");

        var result = await GetArtistService().MergeArtistsAsync(targetArtist.Id, [999999]);

        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist to merge", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    [Fact]
    public async Task MergeArtistsAsync_EmptySourceArray_ThrowsArgumentException()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            GetArtistService().MergeArtistsAsync(targetArtist.Id, []));
    }

    [Fact]
    public async Task MergeArtistsAsync_InvalidTargetId_ThrowsArgumentException()
    {
        var library = await CreateTestLibrary();
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            GetArtistService().MergeArtistsAsync(0, [sourceArtist.Id]));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            GetArtistService().MergeArtistsAsync(-1, [sourceArtist.Id]));
    }

    #endregion

    #region Complex Merge Scenarios

    [Fact]
    public async Task MergeArtistsAsync_CompleteDataMerge_AllElementsTransferred()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");
        var relatedArtist = await CreateArtistInLibrary(library, "Related Artist");

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var dbSource = await context.Artists.FindAsync(sourceArtist.Id);
            dbSource!.AlternateNames = "SOURCEALIAS|ANOTHERALIAS";
            dbSource.MusicBrainzId = Guid.NewGuid();
            dbSource.SpotifyId = "spotify:artist:source123";
            await context.SaveChangesAsync();
        }

        var user = await CreateTestUser();
        var sourceAlbum = await CreateAlbumForArtist(sourceArtist, "Source Album");
        var sourceSong = await CreateSongForAlbum(sourceAlbum, "Source Song");

        await CreateUserArtist(user, sourceArtist, isStarred: true, rating: 5);
        await CreateUserPin(user, sourceArtist);
        await CreateContributor(sourceArtist, sourceAlbum, sourceSong, ContributorType.Performer);
        await CreateArtistRelation(sourceArtist, relatedArtist, ArtistRelationType.Similar);
        await CreateArtistRelation(relatedArtist, sourceArtist, ArtistRelationType.Associated);

        var result = await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist.Id]);

        AssertResultIsSuccessful(result);

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            Assert.Null(await context.Artists.FindAsync(sourceArtist.Id));

            var updatedTarget = await context.Artists.FindAsync(targetArtist.Id);
            Assert.NotNull(updatedTarget);
            Assert.Contains("SOURCEALIAS", updatedTarget.AlternateNames ?? string.Empty);

            var albums = context.Albums.Where(a => a.ArtistId == targetArtist.Id).ToList();
            Assert.Single(albums);
            Assert.Equal("Source Album", albums[0].Name);

            var userArtists = context.UserArtists.Where(ua => ua.ArtistId == targetArtist.Id).ToList();
            Assert.Single(userArtists);

            var userPins = context.UserPins.Where(up => up.PinId == targetArtist.Id && up.PinType == (int)UserPinType.Artist).ToList();
            Assert.Single(userPins);

            var contributors = context.Contributors.Where(c => c.ArtistId == targetArtist.Id).ToList();
            Assert.Single(contributors);

            var outboundRelations = context.ArtistRelation.Where(ar => ar.ArtistId == targetArtist.Id).ToList();
            Assert.Single(outboundRelations);

            var inboundRelations = context.ArtistRelation.Where(ar => ar.RelatedArtistId == targetArtist.Id).ToList();
            Assert.Single(inboundRelations);
        }
    }

    [Fact]
    public async Task MergeArtistsAsync_MergeThreeArtistsIntoOne_AllDataConsolidated()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist1 = await CreateArtistInLibrary(library, "Source Artist 1");
        var sourceArtist2 = await CreateArtistInLibrary(library, "Source Artist 2");

        var user1 = await CreateTestUser("user1@test.com");
        var user2 = await CreateTestUser("user2@test.com");

        var album1 = await CreateAlbumForArtist(sourceArtist1, "Album 1");
        var album2 = await CreateAlbumForArtist(sourceArtist2, "Album 2");
        var targetAlbum = await CreateAlbumForArtist(targetArtist, "Target Album");

        var song1 = await CreateSongForAlbum(album1, "Song 1");
        var song2 = await CreateSongForAlbum(album2, "Song 2");
        var targetSong = await CreateSongForAlbum(targetAlbum, "Target Song");

        await CreateUserArtist(user1, sourceArtist1, isStarred: true, rating: 4);
        await CreateUserArtist(user2, sourceArtist2, isStarred: true, rating: 3);

        await CreateUserPin(user1, sourceArtist1);
        await CreateUserPin(user2, sourceArtist2);

        var result = await GetArtistService().MergeArtistsAsync(targetArtist.Id, [sourceArtist1.Id, sourceArtist2.Id]);

        AssertResultIsSuccessful(result);

        await using var context = await MockFactory().CreateDbContextAsync();
        Assert.Null(await context.Artists.FindAsync(sourceArtist1.Id));
        Assert.Null(await context.Artists.FindAsync(sourceArtist2.Id));
        Assert.NotNull(await context.Artists.FindAsync(targetArtist.Id));

        var albums = context.Albums.Where(a => a.ArtistId == targetArtist.Id).ToList();
        Assert.Equal(3, albums.Count);

        var songs = context.Songs.Where(s => s.Album.ArtistId == targetArtist.Id).ToList();
        Assert.Equal(3, songs.Count);

        var userPins = context.UserPins
            .Where(up => up.PinId == targetArtist.Id && up.PinType == (int)UserPinType.Artist)
            .ToList();
        Assert.Equal(2, userPins.Count);
    }

    #endregion

    #region Helper Methods

    private async Task<(Artist target, Artist source, Library library)> CreateMergeTestArtists()
    {
        var library = await CreateTestLibrary();
        var targetArtist = await CreateArtistInLibrary(library, "Target Artist");
        var sourceArtist = await CreateArtistInLibrary(library, "Source Artist");
        return (targetArtist, sourceArtist, library);
    }

    private async Task<Library> CreateTestLibrary()
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var library = new Library
        {
            Name = $"Test Library {Guid.NewGuid():N}",
            Path = $"/test/library/{Guid.NewGuid():N}",
            Type = (int)LibraryType.Storage,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Libraries.Add(library);
        await context.SaveChangesAsync();
        return library;
    }

    private async Task<Artist> CreateArtistInLibrary(Library library, string artistName)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var artist = new Artist
        {
            ApiKey = Guid.NewGuid(),
            Directory = $"{artistName.ToNormalizedString()}-{Guid.NewGuid():N}",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            LibraryId = library.Id,
            Name = artistName,
            NameNormalized = artistName.ToNormalizedString()!
        };
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        return artist;
    }

    private async Task<Album> CreateAlbumForArtist(Artist artist, string albumName)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var album = new Album
        {
            ApiKey = Guid.NewGuid(),
            ArtistId = artist.Id,
            Directory = $"{albumName.ToNormalizedString()}-{Guid.NewGuid():N}",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            Name = albumName,
            NameNormalized = albumName.ToNormalizedString()!,
            SongCount = 0,
            Duration = 0,
            ReleaseDate = LocalDate.FromDateTime(DateTime.Today),
            AlbumStatus = (short)AlbumStatus.Ok
        };
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    private async Task<Song> CreateSongForAlbum(Album album, string songTitle)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var existingSongCount = context.Songs.Count(s => s.AlbumId == album.Id);
        var song = new Song
        {
            ApiKey = Guid.NewGuid(),
            AlbumId = album.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            Title = songTitle,
            TitleNormalized = songTitle.ToNormalizedString()!,
            SongNumber = existingSongCount + 1,
            Duration = 180,
            FileSize = 1024000,
            FileName = $"{songTitle.ToNormalizedString()}.mp3",
            FileHash = Guid.NewGuid().ToString("N"),
            BitRate = 320,
            BitDepth = 16,
            SamplingRate = 44100,
            BPM = 120,
            ContentType = "audio/mpeg"
        };
        context.Songs.Add(song);
        await context.SaveChangesAsync();
        return song;
    }

    private async Task<User> CreateTestUser(string email = "test@test.com")
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var user = new User
        {
            ApiKey = Guid.NewGuid(),
            UserName = email.Split('@')[0] + uniqueId,
            UserNameNormalized = (email.Split('@')[0] + uniqueId).ToUpperInvariant(),
            Email = $"{uniqueId}_{email}",
            EmailNormalized = $"{uniqueId}_{email}".ToUpperInvariant(),
            PublicKey = Guid.NewGuid().ToString(),
            PasswordEncrypted = "encryptedpassword",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<UserArtist> CreateUserArtist(User user, Artist artist, bool isStarred = false, int rating = 0)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var userArtist = new UserArtist
        {
            ApiKey = Guid.NewGuid(),
            UserId = user.Id,
            ArtistId = artist.Id,
            IsStarred = isStarred,
            Rating = rating,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.UserArtists.Add(userArtist);
        await context.SaveChangesAsync();
        return userArtist;
    }

    private async Task<UserPin> CreateUserPin(User user, Artist artist)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var userPin = new UserPin
        {
            ApiKey = Guid.NewGuid(),
            UserId = user.Id,
            PinType = (int)UserPinType.Artist,
            PinId = artist.Id,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.UserPins.Add(userPin);
        await context.SaveChangesAsync();
        return userPin;
    }

    private static int _contributorMetaTagCounter = 100;

    private async Task<Contributor> CreateContributor(Artist artist, Album album, Song song, ContributorType contributorType)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var metaTagId = Interlocked.Increment(ref _contributorMetaTagCounter);
        var contributor = new Contributor
        {
            ApiKey = Guid.NewGuid(),
            ArtistId = artist.Id,
            AlbumId = album.Id,
            SongId = song.Id,
            Role = contributorType.ToString(),
            ContributorType = (int)contributorType,
            ContributorName = artist.Name,
            MetaTagIdentifier = metaTagId,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Contributors.Add(contributor);
        await context.SaveChangesAsync();
        return contributor;
    }

    private async Task<ArtistRelation> CreateArtistRelation(Artist artist, Artist relatedArtist, ArtistRelationType relationType)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var relation = new ArtistRelation
        {
            ApiKey = Guid.NewGuid(),
            ArtistId = artist.Id,
            RelatedArtistId = relatedArtist.Id,
            ArtistRelationType = (int)relationType,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.ArtistRelation.Add(relation);
        await context.SaveChangesAsync();
        return relation;
    }

    #endregion
}
