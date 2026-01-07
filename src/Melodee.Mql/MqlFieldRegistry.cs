using Melodee.Mql.Models;

namespace Melodee.Mql;

/// <summary>
/// Registry of all fields available for MQL queries across all entity types.
/// </summary>
public static class MqlFieldRegistry
{
    private static readonly Dictionary<string, Dictionary<string, MqlFieldInfo>> _entityFields = new()
    {
        ["songs"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = new("title", [], MqlFieldType.String, "Song.TitleNormalized", false, "Song title"),
            ["artist"] = new("artist", [], MqlFieldType.String, "Song.Album.Artist.NameNormalized", false, "Artist name"),
            ["album"] = new("album", [], MqlFieldType.String, "Song.Album.NameNormalized", false, "Album name"),
            ["genre"] = new("genre", [], MqlFieldType.ArrayString, "Song.Genres", false, "Genre tags"),
            ["mood"] = new("mood", [], MqlFieldType.ArrayString, "Song.Moods", false, "Mood tags"),
            ["year"] = new("year", [], MqlFieldType.Number, "Song.Album.ReleaseDate.Year", false, "Release year"),
            ["duration"] = new("duration", [], MqlFieldType.Number, "Song.Duration", false, "Duration in seconds"),
            ["bpm"] = new("bpm", [], MqlFieldType.Number, "Song.BPM", false, "Beats per minute"),
            ["rating"] = new("rating", [], MqlFieldType.Number, "Song.UserSongs.Rating", true, "User rating"),
            ["plays"] = new("plays", [], MqlFieldType.Number, "Song.UserSongs.PlayedCount", true, "Play count"),
            ["starred"] = new("starred", [], MqlFieldType.Boolean, "Song.UserSongs.IsStarred", true, "Starred status"),
            ["starredat"] = new("starredAt", [], MqlFieldType.Date, "Song.UserSongs.StarredAt", true, "Date starred"),
            ["lastplayedat"] = new("lastPlayedAt", [], MqlFieldType.Date, "Song.UserSongs.LastPlayedAt", true, "Last played date"),
            ["added"] = new("added", [], MqlFieldType.Date, "Song.CreatedAt", false, "Date added to library"),
            ["composer"] = new("composer", [], MqlFieldType.String, "Song.ComposerNormalized", false, "Composer name"),
            ["discnumber"] = new("discNumber", ["disc"], MqlFieldType.Number, "Song.DiscNumber", false, "Disc number"),
            ["tracknumber"] = new("trackNumber", ["track"], MqlFieldType.Number, "Song.SortOrder", false, "Track number"),
            ["comment"] = new("comment", [], MqlFieldType.String, "Song.Comment", false, "Comment"),
            ["imagecount"] = new("imageCount", ["images"], MqlFieldType.Number, "Song.ImageCount", false, "Number of images")
        },
        ["albums"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["album"] = new("album", ["name"], MqlFieldType.String, "Album.NameNormalized", false, "Album name"),
            ["artist"] = new("artist", [], MqlFieldType.String, "Album.Artist.NameNormalized", false, "Artist name"),
            ["year"] = new("year", [], MqlFieldType.Number, "Album.ReleaseDate.Year", false, "Release year"),
            ["duration"] = new("duration", [], MqlFieldType.Number, "Album.Duration", false, "Total duration in seconds"),
            ["genre"] = new("genre", [], MqlFieldType.ArrayString, "Album.Genres", false, "Genre tags"),
            ["mood"] = new("mood", [], MqlFieldType.ArrayString, "Album.Moods", false, "Mood tags"),
            ["rating"] = new("rating", [], MqlFieldType.Number, "Album.UserAlbums.Rating", true, "User rating"),
            ["plays"] = new("plays", [], MqlFieldType.Number, "Album.UserAlbums.PlayedCount", true, "Play count"),
            ["starred"] = new("starred", [], MqlFieldType.Boolean, "Album.UserAlbums.IsStarred", true, "Starred status"),
            ["starredat"] = new("starredAt", [], MqlFieldType.Date, "Album.UserAlbums.StarredAt", true, "Date starred"),
            ["lastplayedat"] = new("lastPlayedAt", [], MqlFieldType.Date, "Album.UserAlbums.LastPlayedAt", true, "Last played date"),
            ["added"] = new("added", [], MqlFieldType.Date, "Album.CreatedAt", false, "Date added to library"),
            ["originalyear"] = new("originalYear", ["origyear"], MqlFieldType.Number, "Album.OrigAlbumYear", false, "Original release year"),
            ["songcount"] = new("songCount", ["trackcount"], MqlFieldType.Number, "Album.SongCount", false, "Number of songs")
        },
        ["artists"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["artist"] = new("artist", ["name"], MqlFieldType.String, "Artist.NameNormalized", false, "Artist name"),
            ["rating"] = new("rating", [], MqlFieldType.Number, "Artist.UserArtists.Rating", true, "User rating"),
            ["starred"] = new("starred", [], MqlFieldType.Boolean, "Artist.UserArtists.IsStarred", true, "Starred status"),
            ["starredat"] = new("starredAt", [], MqlFieldType.Date, "Artist.UserArtists.StarredAt", true, "Date starred"),
            ["plays"] = new("plays", [], MqlFieldType.Number, "Artist.PlayedCount", false, "Total play count"),
            ["added"] = new("added", [], MqlFieldType.Date, "Artist.CreatedAt", false, "Date added to library"),
            ["songcount"] = new("songCount", [], MqlFieldType.Number, "Artist.SongCount", false, "Number of songs"),
            ["albumcount"] = new("albumCount", [], MqlFieldType.Number, "Artist.AlbumCount", false, "Number of albums")
        }
    };

    /// <summary>
    /// Gets all field names available for a given entity type.
    /// </summary>
    public static IEnumerable<string> GetFieldNames(string entityType)
    {
        return _entityFields.TryGetValue(entityType.ToLowerInvariant(), out var fields)
            ? fields.Values.Select(f => f.Name)
            : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Gets field information for a specific field name and entity type.
    /// </summary>
    public static MqlFieldInfo? GetField(string fieldName, string entityType)
    {
        var normalizedEntity = entityType.ToLowerInvariant();
        if (!_entityFields.TryGetValue(normalizedEntity, out var fields))
        {
            return null;
        }

        var normalizedField = fieldName.ToLowerInvariant();
        if (fields.TryGetValue(normalizedField, out var field))
        {
            return field;
        }

        foreach (var kvp in fields)
        {
            if (kvp.Value.Matches(fieldName))
            {
                return kvp.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if a field exists for the given entity type.
    /// </summary>
    public static bool FieldExists(string fieldName, string entityType)
    {
        return GetField(fieldName, entityType) is not null;
    }

    /// <summary>
    /// Gets all fields that are user-scoped for a given entity type.
    /// </summary>
    public static IEnumerable<MqlFieldInfo> GetUserScopedFields(string entityType)
    {
        return GetFieldInfos(entityType).Where(f => f.IsUserScoped);
    }

    /// <summary>
    /// Gets all field information for a given entity type.
    /// </summary>
    public static IEnumerable<MqlFieldInfo> GetFieldInfos(string entityType)
    {
        return _entityFields.TryGetValue(entityType.ToLowerInvariant(), out var fields)
            ? fields.Values
            : Enumerable.Empty<MqlFieldInfo>();
    }

    /// <summary>
    /// Gets all supported entity types.
    /// </summary>
    public static IEnumerable<string> GetEntityTypes()
    {
        return _entityFields.Keys;
    }

    /// <summary>
    /// Gets fields that support a specific operator.
    /// </summary>
    public static IEnumerable<MqlFieldInfo> GetFieldsForOperator(string entityType, string op)
    {
        return GetFieldInfos(entityType).Where(f =>
        {
            var type = f.Type;
            return op switch
            {
                ":=" or ":!=" or ":<" or ":<=" or ":>" or ":>=" => type == MqlFieldType.Number || type == MqlFieldType.Date,
                "contains" or "startswith" or "endswith" or "wildcard" => type == MqlFieldType.String || type == MqlFieldType.ArrayString,
                _ => true
            };
        });
    }
}
