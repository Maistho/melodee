using FluentAssertions;
using Melodee.Mql.Authorization;

namespace Melodee.Mql.Tests;

public class MqlAuthorizationTests
{
    private readonly MqlAuthorizationService _authorizationService;

    public MqlAuthorizationTests()
    {
        _authorizationService = new MqlAuthorizationService();
    }

    [Fact]
    public void AuthorizeQuery_EmptyQuery_ReturnsSuccess()
    {
        var result = _authorizationService.AuthorizeQuery("", "songs", 1);

        result.IsAuthorized.Should().BeTrue();
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void AuthorizeQuery_NullQuery_ReturnsSuccess()
    {
        var result = _authorizationService.AuthorizeQuery(null!, "songs", 1);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeQuery_NonUserScopedField_ReturnsSuccess()
    {
        var result = _authorizationService.AuthorizeQuery("artist:Pink Floyd", "songs", null);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeQuery_AnonymousUserBlockedFromUserScopedField()
    {
        var result = _authorizationService.AuthorizeQuery("rating:>3", "songs", null);

        result.IsAuthorized.Should().BeFalse();
        result.ErrorCode.Should().Be("MQL_FORBIDDEN_FIELD");
        result.FieldName.Should().Be("rating");
    }

    [Fact]
    public void AuthorizeQuery_AuthenticatedUserCanAccessOwnData()
    {
        var result = _authorizationService.AuthorizeQuery("rating:>3", "songs", 1);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeQuery_CrossUserQueryBlocked()
    {
        var result = _authorizationService.AuthorizeQuery("rating:>3", "songs", 1, targetUserId: 2);

        result.IsAuthorized.Should().BeFalse();
        result.ErrorCode.Should().Be("MQL_FORBIDDEN_USER_DATA");
    }

    [Fact]
    public void AuthorizeQuery_SameUserIdAllowed()
    {
        var result = _authorizationService.AuthorizeQuery("rating:>3", "songs", 1, targetUserId: 1);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeQuery_AlbumUserScopedFieldBlockedForAnonymous()
    {
        var result = _authorizationService.AuthorizeQuery("plays:>100", "albums", null);

        result.IsAuthorized.Should().BeFalse();
        result.FieldName.Should().Be("plays");
    }

    [Fact]
    public void AuthorizeQuery_ArtistUserScopedFieldBlockedForAnonymous()
    {
        var result = _authorizationService.AuthorizeQuery("starred:true", "artists", null);

        result.IsAuthorized.Should().BeFalse();
        result.FieldName.Should().Be("starred");
    }

    [Fact]
    public void AuthorizeQuery_AuthenticatedUserCanQueryArtistData()
    {
        var result = _authorizationService.AuthorizeQuery("starred:true", "artists", 1);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeQuery_MultipleUserScopedFieldsBlockedForAnonymous()
    {
        var result = _authorizationService.AuthorizeQuery("rating:>3 and starred:true", "songs", null);

        result.IsAuthorized.Should().BeFalse();
        result.BlockedFields.Should().HaveCount(2);
        result.BlockedFields.Should().Contain("rating");
        result.BlockedFields.Should().Contain("starred");
    }

    [Fact]
    public void AuthorizeQuery_MixedFields_PartialAllowed()
    {
        var result = _authorizationService.AuthorizeQuery("artist:Pink Floyd", "songs", null);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void IsUserScopedField_SongsRating_ReturnsTrue()
    {
        var result = _authorizationService.IsUserScopedField("rating", "songs");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsUserScopedField_SongsArtist_ReturnsFalse()
    {
        var result = _authorizationService.IsUserScopedField("artist", "songs");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsUserScopedField_AlbumsRating_ReturnsTrue()
    {
        var result = _authorizationService.IsUserScopedField("rating", "albums");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsUserScopedField_ArtistsStarred_ReturnsTrue()
    {
        var result = _authorizationService.IsUserScopedField("starred", "artists");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsUserScopedField_UnknownField_ReturnsFalse()
    {
        var result = _authorizationService.IsUserScopedField("unknownfield", "songs");

        result.Should().BeFalse();
    }

    [Fact]
    public void GetUserScopedFields_Songs_ReturnsCorrectFields()
    {
        var fields = _authorizationService.GetUserScopedFields("songs");

        fields.Should().Contain("rating");
        fields.Should().Contain("plays");
        fields.Should().Contain("starred");
        fields.Should().Contain("starredat");
        fields.Should().Contain("lastplayedat");
    }

    [Fact]
    public void GetUserScopedFields_Albums_ReturnsCorrectFields()
    {
        var fields = _authorizationService.GetUserScopedFields("albums");

        fields.Should().Contain("rating");
        fields.Should().Contain("plays");
        fields.Should().Contain("starred");
        fields.Should().Contain("starredat");
        fields.Should().Contain("lastplayedat");
    }

    [Fact]
    public void GetUserScopedFields_Artists_ReturnsCorrectFields()
    {
        var fields = _authorizationService.GetUserScopedFields("artists");

        fields.Should().Contain("rating");
        fields.Should().Contain("starred");
        fields.Should().Contain("starredat");
        fields.Should().NotContain("plays");
    }

    [Fact]
    public void AuthorizeFieldAccess_NonUserScopedField_ReturnsSuccess()
    {
        var result = _authorizationService.AuthorizeFieldAccess("artist", "songs", 1);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeFieldAccess_UserScopedFieldWithUser_ReturnsSuccess()
    {
        var result = _authorizationService.AuthorizeFieldAccess("rating", "songs", 1);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void AuthorizeFieldAccess_UserScopedFieldWithoutUser_ReturnsForbidden()
    {
        var result = _authorizationService.AuthorizeFieldAccess("rating", "songs", null);

        result.IsAuthorized.Should().BeFalse();
        result.ErrorCode.Should().Be("MQL_FORBIDDEN_FIELD");
    }

    [Fact]
    public void AuthorizeFieldAccess_CrossUserBlocked()
    {
        var result = _authorizationService.AuthorizeFieldAccess("rating", "songs", 1, 2);

        result.IsAuthorized.Should().BeFalse();
        result.ErrorCode.Should().Be("MQL_FORBIDDEN_USER_DATA");
    }

    [Fact]
    public void AuthorizeFieldAccess_CaseInsensitive()
    {
        var result = _authorizationService.AuthorizeFieldAccess("RATING", "songs", null);

        result.IsAuthorized.Should().BeFalse();
        result.FieldName.Should().Be("RATING");
    }

    [Fact]
    public void AuthorizeQuery_LastPlayedAtFieldBlockedForAnonymous()
    {
        var result = _authorizationService.AuthorizeQuery("lastplayedat:>2024-01-01", "songs", null);

        result.IsAuthorized.Should().BeFalse();
        result.FieldName.Should().Be("lastplayedat");
    }

    [Fact]
    public void AuthorizeQuery_StarredAtFieldBlockedForAnonymous()
    {
        var result = _authorizationService.AuthorizeQuery("starredat:>2024-01-01", "songs", null);

        result.IsAuthorized.Should().BeFalse();
        result.FieldName.Should().Be("starredat");
    }

    [Fact]
    public void AuthorizeQuery_WithNotOperator_BlockedForAnonymous()
    {
        var result = _authorizationService.AuthorizeQuery("not:rating:>3", "songs", null);

        result.IsAuthorized.Should().BeFalse();
        result.FieldName.Should().Be("rating");
    }

    [Fact]
    public void AuthorizeQuery_WithComplexQuery_BlockedForAnonymous()
    {
        var result = _authorizationService.AuthorizeQuery(
            "rating:>3 and starred:true",
            "songs",
            null);

        result.IsAuthorized.Should().BeFalse();
        result.BlockedFields.Should().HaveCount(2);
        result.BlockedFields.Should().Contain("rating");
        result.BlockedFields.Should().Contain("starred");
    }
}
