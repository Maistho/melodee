using FluentAssertions;
using Melodee.Mql.Models;

namespace Melodee.Mql.Tests;

public class MqlFieldRegistryTests
{
    [Fact]
    public void GetFieldNames_Songs_ReturnsAllSongFields()
    {
        var fields = MqlFieldRegistry.GetFieldNames("songs").ToList();

        fields.Should().Contain("title");
        fields.Should().Contain("artist");
        fields.Should().Contain("album");
        fields.Should().Contain("genre");
        fields.Should().Contain("year");
        fields.Should().Contain("duration");
        fields.Should().Contain("bpm");
        fields.Should().Contain("rating");
        fields.Should().Contain("plays");
        fields.Should().Contain("starred");
        fields.Should().Contain("starredAt");
        fields.Should().Contain("lastPlayedAt");
    }

    [Fact]
    public void GetFieldNames_Albums_ReturnsAllAlbumFields()
    {
        var fields = MqlFieldRegistry.GetFieldNames("albums").ToList();

        fields.Should().Contain("album");
        fields.Should().Contain("artist");
        fields.Should().Contain("year");
        fields.Should().Contain("duration");
        fields.Should().Contain("genre");
        fields.Should().Contain("rating");
        fields.Should().Contain("starred");
    }

    [Fact]
    public void GetFieldNames_Artists_ReturnsAllArtistFields()
    {
        var fields = MqlFieldRegistry.GetFieldNames("artists").ToList();

        fields.Should().Contain("artist");
        fields.Should().Contain("rating");
        fields.Should().Contain("starred");
        fields.Should().Contain("plays");
    }

    [Fact]
    public void GetField_ValidField_ReturnsFieldInfo()
    {
        var field = MqlFieldRegistry.GetField("title", "songs");

        field.Should().NotBeNull();
        field!.Name.Should().Be("title");
        field.Type.Should().Be(MqlFieldType.String);
        field.DbMapping.Should().Be("Song.TitleNormalized");
        field.IsUserScoped.Should().BeFalse();
    }

    [Fact]
    public void GetField_FieldAlias_ReturnsFieldInfo()
    {
        var field = MqlFieldRegistry.GetField("disc", "songs");

        field.Should().NotBeNull();
        field!.Name.Should().Be("discNumber");
        field.Aliases.Should().Contain("disc");
    }

    [Fact]
    public void GetField_InvalidField_ReturnsNull()
    {
        var field = MqlFieldRegistry.GetField("invalidfield", "songs");

        field.Should().BeNull();
    }

    [Fact]
    public void GetField_InvalidEntity_ReturnsNull()
    {
        var field = MqlFieldRegistry.GetField("title", "invalid");

        field.Should().BeNull();
    }

    [Fact]
    public void FieldExists_ValidField_ReturnsTrue()
    {
        MqlFieldRegistry.FieldExists("title", "songs").Should().BeTrue();
        MqlFieldRegistry.FieldExists("rating", "songs").Should().BeTrue();
        MqlFieldRegistry.FieldExists("album", "albums").Should().BeTrue();
    }

    [Fact]
    public void FieldExists_InvalidField_ReturnsFalse()
    {
        MqlFieldRegistry.FieldExists("invalid", "songs").Should().BeFalse();
    }

    [Fact]
    public void GetUserScopedFields_Songs_ReturnsUserScopedFields()
    {
        var fields = MqlFieldRegistry.GetUserScopedFields("songs").ToList();

        fields.Should().Contain(f => f.Name == "rating");
        fields.Should().Contain(f => f.Name == "plays");
        fields.Should().Contain(f => f.Name == "starred");
        fields.Should().Contain(f => f.Name == "starredAt");
        fields.Should().Contain(f => f.Name == "lastPlayedAt");
        fields.Should().NotContain(f => f.Name == "title");
        fields.Should().NotContain(f => f.Name == "artist");
    }

    [Fact]
    public void GetUserScopedFields_Albums_ReturnsUserScopedFields()
    {
        var fields = MqlFieldRegistry.GetUserScopedFields("albums").ToList();

        fields.Should().Contain(f => f.Name == "rating");
        fields.Should().Contain(f => f.Name == "plays");
        fields.Should().Contain(f => f.Name == "starred");
    }

    [Fact]
    public void GetUserScopedFields_Artists_ReturnsUserScopedFields()
    {
        var fields = MqlFieldRegistry.GetUserScopedFields("artists").ToList();

        fields.Should().Contain(f => f.Name == "rating");
        fields.Should().Contain(f => f.Name == "starred");
        fields.Should().NotContain(f => f.Name == "plays");
    }

    [Fact]
    public void GetFieldInfos_ReturnsAllFieldInfos()
    {
        var fields = MqlFieldRegistry.GetFieldInfos("songs").ToList();

        fields.Should().NotBeEmpty();
        fields.Should().AllBeOfType<MqlFieldInfo>();
    }

    [Fact]
    public void GetEntityTypes_ReturnsAllSupportedTypes()
    {
        var types = MqlFieldRegistry.GetEntityTypes().ToList();

        types.Should().Contain("songs");
        types.Should().Contain("albums");
        types.Should().Contain("artists");
        types.Count().Should().Be(3);
    }

    [Fact]
    public void FieldInfo_Matches_ReturnsTrueForName()
    {
        var field = MqlFieldRegistry.GetField("discNumber", "songs");

        field.Should().NotBeNull();
        field!.Matches("discNumber").Should().BeTrue();
    }

    [Fact]
    public void FieldInfo_Matches_ReturnsTrueForAlias()
    {
        var field = MqlFieldRegistry.GetField("discNumber", "songs");

        field.Should().NotBeNull();
        field!.Matches("disc").Should().BeTrue();
    }

    [Fact]
    public void FieldInfo_Matches_ReturnsFalseForDifferentName()
    {
        var field = MqlFieldRegistry.GetField("title", "songs");

        field.Should().NotBeNull();
        field!.Matches("artist").Should().BeFalse();
    }

    [Theory]
    [InlineData("songs")]
    [InlineData("albums")]
    [InlineData("artists")]
    public void AllFieldsHaveDescription(string entityType)
    {
        var fields = MqlFieldRegistry.GetFieldInfos(entityType).ToList();

        fields.Should().NotBeEmpty();
        foreach (var field in fields)
        {
            field.Description.Should().NotBeNullOrEmpty($"{field.Name} should have a description");
        }
    }

    [Theory]
    [InlineData("songs")]
    [InlineData("albums")]
    [InlineData("artists")]
    public void AllFieldsHaveValidDbMapping(string entityType)
    {
        var fields = MqlFieldRegistry.GetFieldInfos(entityType).ToList();

        fields.Should().NotBeEmpty();
        foreach (var field in fields)
        {
            field.DbMapping.Should().NotBeNullOrEmpty();
            field.DbMapping.Should().NotContain(" ", $"{field.Name} should not have spaces in DbMapping");
        }
    }

    [Theory]
    [InlineData("songs")]
    [InlineData("albums")]
    [InlineData("artists")]
    public void FieldNamesAreNormalized(string entityType)
    {
        var fields = MqlFieldRegistry.GetFieldNames(entityType).ToList();
        fields.Should().NotBeEmpty();

        foreach (var field in fields)
        {
            field.Should().NotContain(" ", $"{field} should not contain spaces");
            field.Should().MatchRegex(@"^[a-zA-Z][a-zA-Z0-9]*$", $"{field} should be a valid identifier");
        }
    }
}
