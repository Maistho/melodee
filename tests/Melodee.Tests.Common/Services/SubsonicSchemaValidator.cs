using System.Text.Json;
using System.Text.Json.Nodes;

namespace Melodee.Tests.Common.Services;

public static class SubsonicSchemaValidator
{
    private static readonly Dictionary<string, ElementDefinition> ElementDefinitions = new()
    {
        ["musicFolders"] = new ElementDefinition
        {
            TypeName = "MusicFolders",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["musicFolder"] = new FieldDefinition { Type = FieldType.Array, ItemType = "MusicFolder" }
            },
            RequiredAttributes = Array.Empty<string>()
        },
        ["MusicFolder"] = new ElementDefinition
        {
            TypeName = "MusicFolder",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.Integer,
                ["name"] = FieldType.String
            },
            RequiredAttributes = new[] { "id" }
        },
        ["indexes"] = new ElementDefinition
        {
            TypeName = "Indexes",
            Attributes = new Dictionary<string, FieldType>
            {
                ["lastModified"] = FieldType.Long,
                ["ignoredArticles"] = FieldType.String
            },
            RequiredAttributes = new[] { "lastModified", "ignoredArticles" }
        },
        ["index"] = new ElementDefinition
        {
            TypeName = "Index",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["artist"] = new FieldDefinition { Type = FieldType.Array, ItemType = "artist" }
            }
        },
        ["artist"] = new ElementDefinition
        {
            TypeName = "Artist",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["name"] = FieldType.String,
                ["albumCount"] = FieldType.Integer
            },
            RequiredAttributes = new[] { "id", "name" }
        },
        ["artists"] = new ElementDefinition
        {
            TypeName = "ArtistsID3",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["index"] = new FieldDefinition { Type = FieldType.Array, ItemType = "index" }
            }
        },
        ["album"] = new ElementDefinition
        {
            TypeName = "AlbumWithSongsID3",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["name"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["artistId"] = FieldType.String,
                ["year"] = FieldType.Integer,
                ["genre"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["songCount"] = FieldType.Integer,
                ["duration"] = FieldType.Integer,
                ["created"] = FieldType.DateTime,
                ["albumArtist"] = FieldType.String,
                ["albumArtistId"] = FieldType.String
            },
            RequiredAttributes = new[] { "id", "name" }
        },
        ["song"] = new ElementDefinition
        {
            TypeName = "Child",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["parent"] = FieldType.String,
                ["title"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["album"] = FieldType.String,
                ["genre"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["duration"] = FieldType.Integer,
                ["bitRate"] = FieldType.Integer,
                ["path"] = FieldType.String,
                ["fileSize"] = FieldType.Long,
                ["isDir"] = FieldType.Boolean,
                ["albumId"] = FieldType.String,
                ["artistId"] = FieldType.String,
                ["year"] = FieldType.Integer,
                ["track"] = FieldType.Integer,
                ["discNumber"] = FieldType.Integer,
                ["created"] = FieldType.DateTime,
                ["releaseDate"] = FieldType.DateTime,
                ["genreId"] = FieldType.String,
                ["crc32"] = FieldType.String,
                ["suffix"] = FieldType.String,
                ["contentType"] = FieldType.String,
                ["size"] = FieldType.Long
            },
            RequiredAttributes = new[] { "id" }
        },
        ["genres"] = new ElementDefinition
        {
            TypeName = "Genres",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["genre"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Genre" }
            }
        },
        ["Genre"] = new ElementDefinition
        {
            TypeName = "Genre",
            Attributes = new Dictionary<string, FieldType>
            {
                ["name"] = FieldType.String,
                ["songCount"] = FieldType.Integer,
                ["albumCount"] = FieldType.Integer
            },
            RequiredAttributes = new[] { "name" }
        },
        ["directory"] = new ElementDefinition
        {
            TypeName = "Directory",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["name"] = FieldType.String,
                ["parent"] = FieldType.String,
                ["path"] = FieldType.String
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["child"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            },
            RequiredAttributes = new[] { "id", "name" }
        },
        ["license"] = new ElementDefinition
        {
            TypeName = "License",
            Attributes = new Dictionary<string, FieldType>
            {
                ["valid"] = FieldType.Boolean,
                ["email"] = FieldType.String,
                ["key"] = FieldType.String,
                ["expires"] = FieldType.DateTime,
                ["trial"] = FieldType.Boolean
            },
            RequiredAttributes = new[] { "valid" }
        },
        ["playlists"] = new ElementDefinition
        {
            TypeName = "Playlists",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["playlist"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Playlist" }
            }
        },
        ["Playlist"] = new ElementDefinition
        {
            TypeName = "Playlist",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.Integer,
                ["name"] = FieldType.String,
                ["owner"] = FieldType.String,
                ["public"] = FieldType.Boolean,
                ["songCount"] = FieldType.Integer,
                ["duration"] = FieldType.Integer,
                ["created"] = FieldType.DateTime,
                ["changed"] = FieldType.DateTime,
                ["comment"] = FieldType.String
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            },
            RequiredAttributes = new[] { "id", "name" }
        },
        ["playlist"] = new ElementDefinition
        {
            TypeName = "PlaylistWithSongs",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.Integer,
                ["name"] = FieldType.String,
                ["owner"] = FieldType.String,
                ["public"] = FieldType.Boolean,
                ["songCount"] = FieldType.Integer,
                ["duration"] = FieldType.Integer,
                ["created"] = FieldType.DateTime,
                ["changed"] = FieldType.DateTime,
                ["comment"] = FieldType.String
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            },
            RequiredAttributes = new[] { "id", "name" }
        },
        ["lyrics"] = new ElementDefinition
        {
            TypeName = "Lyrics",
            Attributes = new Dictionary<string, FieldType>
            {
                ["artist"] = FieldType.String,
                ["title"] = FieldType.String
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["verse"] = new FieldDefinition { Type = FieldType.Array, ItemType = "LyricsVerse" }
            }
        },
        ["starred"] = new ElementDefinition
        {
            TypeName = "Starred",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["artist"] = new FieldDefinition { Type = FieldType.Array, ItemType = "artist" },
                ["album"] = new FieldDefinition { Type = FieldType.Array, ItemType = "AlbumChild" },
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            }
        },
        ["starred2"] = new ElementDefinition
        {
            TypeName = "Starred2",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["artist"] = new FieldDefinition { Type = FieldType.Array, ItemType = "artist" },
                ["album"] = new FieldDefinition { Type = FieldType.Array, ItemType = "AlbumChild" },
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            }
        },
        ["nowPlaying"] = new ElementDefinition
        {
            TypeName = "NowPlaying",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["entry"] = new FieldDefinition { Type = FieldType.Array, ItemType = "NowPlayingEntry" }
            }
        },
        ["user"] = new ElementDefinition
        {
            TypeName = "User",
            Attributes = new Dictionary<string, FieldType>
            {
                ["username"] = FieldType.String,
                ["email"] = FieldType.String,
                ["scrobbleEnabled"] = FieldType.Boolean,
                ["adminRole"] = FieldType.Boolean,
                ["settingsRole"] = FieldType.Boolean,
                ["downloadRole"] = FieldType.Boolean,
                ["uploadRole"] = FieldType.Boolean,
                ["playlistRole"] = FieldType.Boolean,
                ["coverArtRole"] = FieldType.Boolean,
                ["commentRole"] = FieldType.Boolean,
                ["podcastRole"] = FieldType.Boolean,
                ["streamRole"] = FieldType.Boolean,
                ["jukeboxRole"] = FieldType.Boolean,
                ["shareRole"] = FieldType.Boolean
            },
            RequiredAttributes = new[] { "username" }
        },
        ["albumList"] = new ElementDefinition
        {
            TypeName = "AlbumList",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["album"] = new FieldDefinition { Type = FieldType.Array, ItemType = "AlbumChild" }
            }
        },
        ["albumList2"] = new ElementDefinition
        {
            TypeName = "AlbumList2",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["album"] = new FieldDefinition { Type = FieldType.Array, ItemType = "AlbumChild" }
            }
        },
        ["AlbumChild"] = new ElementDefinition
        {
            TypeName = "AlbumChild",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["name"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["artistId"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["year"] = FieldType.Integer,
                ["genre"] = FieldType.String,
                ["songCount"] = FieldType.Integer,
                ["duration"] = FieldType.Integer,
                ["created"] = FieldType.DateTime,
                ["albumArtist"] = FieldType.String,
                ["albumArtistId"] = FieldType.String,
                ["parent"] = FieldType.String
            },
            RequiredAttributes = new[] { "id", "name" }
        },
        ["randomSongs"] = new ElementDefinition
        {
            TypeName = "Songs",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            }
        },
        ["searchResult2"] = new ElementDefinition
        {
            TypeName = "SearchResult2",
            Attributes = new Dictionary<string, FieldType>
            {
                ["totalHits"] = FieldType.Integer,
                ["offset"] = FieldType.Integer
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["artist"] = new FieldDefinition { Type = FieldType.Array, ItemType = "artist" },
                ["album"] = new FieldDefinition { Type = FieldType.Array, ItemType = "AlbumChild" },
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            }
        },
        ["searchResult3"] = new ElementDefinition
        {
            TypeName = "SearchResult3",
            Attributes = new Dictionary<string, FieldType>
            {
                ["totalHits"] = FieldType.Integer,
                ["offset"] = FieldType.Integer
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["artist"] = new FieldDefinition { Type = FieldType.Array, ItemType = "artist" },
                ["album"] = new FieldDefinition { Type = FieldType.Array, ItemType = "AlbumChild" },
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            }
        },
        ["songsByGenre"] = new ElementDefinition
        {
            TypeName = "Songs",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            }
        },
        ["artistInfo"] = new ElementDefinition
        {
            TypeName = "ArtistInfo",
            Attributes = new Dictionary<string, FieldType>
            {
                ["name"] = FieldType.String,
                ["bio"] = FieldType.String,
                ["musicBrainzId"] = FieldType.String,
                ["lastFmUrl"] = FieldType.String,
                ["smallImageUrl"] = FieldType.String,
                ["mediumImageUrl"] = FieldType.String,
                ["largeImageUrl"] = FieldType.String
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["similarArtist"] = new FieldDefinition { Type = FieldType.Array, ItemType = "artist" }
            },
            RequiredAttributes = new[] { "name" }
        },
        ["albumInfo"] = new ElementDefinition
        {
            TypeName = "AlbumInfo",
            Attributes = new Dictionary<string, FieldType>
            {
                ["title"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["artistId"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["notes"] = FieldType.String,
                ["musicBrainzId"] = FieldType.String,
                ["lastFmUrl"] = FieldType.String,
                ["smallImageUrl"] = FieldType.String,
                ["mediumImageUrl"] = FieldType.String,
                ["largeImageUrl"] = FieldType.String
            },
            RequiredAttributes = new[] { "title" }
        },
        ["internetRadioStations"] = new ElementDefinition
        {
            TypeName = "InternetRadioStations",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["internetRadioStation"] = new FieldDefinition { Type = FieldType.Array, ItemType = "InternetRadioStation" }
            }
        },
        ["InternetRadioStation"] = new ElementDefinition
        {
            TypeName = "InternetRadioStation",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["name"] = FieldType.String,
                ["streamUrl"] = FieldType.String,
                ["homePageUrl"] = FieldType.String
            },
            RequiredAttributes = new[] { "id", "name", "streamUrl" }
        },
        ["LyricsVerse"] = new ElementDefinition
        {
            TypeName = "LyricsVerse",
            Attributes = new Dictionary<string, FieldType>
            {
                ["nr"] = FieldType.Integer,
                ["value"] = FieldType.String
            },
            RequiredAttributes = new[] { "value" }
        },
        ["Lyrics"] = new ElementDefinition
        {
            TypeName = "Lyrics",
            Attributes = new Dictionary<string, FieldType>
            {
                ["artist"] = FieldType.String,
                ["title"] = FieldType.String
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["verse"] = new FieldDefinition { Type = FieldType.Array, ItemType = "LyricsVerse" }
            }
        },
        ["artists"] = new ElementDefinition
        {
            TypeName = "ArtistsID3",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["index"] = new FieldDefinition { Type = FieldType.Array, ItemType = "index" }
            }
        },
        ["index"] = new ElementDefinition
        {
            TypeName = "Index",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["artist"] = new FieldDefinition { Type = FieldType.Array, ItemType = "artist" }
            }
        },
        ["NowPlayingEntry"] = new ElementDefinition
        {
            TypeName = "NowPlayingEntry",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["parent"] = FieldType.String,
                ["title"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["album"] = FieldType.String,
                ["genre"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["duration"] = FieldType.Integer,
                ["bitRate"] = FieldType.Integer,
                ["path"] = FieldType.String,
                ["fileSize"] = FieldType.Long,
                ["isDir"] = FieldType.Boolean,
                ["albumId"] = FieldType.String,
                ["artistId"] = FieldType.String,
                ["year"] = FieldType.Integer,
                ["track"] = FieldType.Integer,
                ["discNumber"] = FieldType.Integer,
                ["created"] = FieldType.DateTime,
                ["releaseDate"] = FieldType.DateTime,
                ["genreId"] = FieldType.String,
                ["crc32"] = FieldType.String,
                ["suffix"] = FieldType.String,
                ["contentType"] = FieldType.String,
                ["size"] = FieldType.Long,
                ["username"] = FieldType.String,
                ["playerName"] = FieldType.String,
                ["minutesAgo"] = FieldType.Integer,
                ["secondsAgo"] = FieldType.Integer
            },
            RequiredAttributes = new[] { "id", "title" }
        },
        ["nowPlaying"] = new ElementDefinition
        {
            TypeName = "NowPlaying",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["entry"] = new FieldDefinition { Type = FieldType.Array, ItemType = "NowPlayingEntry" }
            }
        },
        ["Share"] = new ElementDefinition
        {
            TypeName = "Share",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.Integer,
                ["url"] = FieldType.String,
                ["description"] = FieldType.String,
                ["username"] = FieldType.String,
                ["created"] = FieldType.DateTime,
                ["expires"] = FieldType.DateTime,
                ["lastVisited"] = FieldType.DateTime,
                ["visitCount"] = FieldType.Integer
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["entry"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            },
            RequiredAttributes = new[] { "id", "url" }
        },
        ["shares"] = new ElementDefinition
        {
            TypeName = "Shares",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["share"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Share" }
            }
        },
        ["Bookmark"] = new ElementDefinition
        {
            TypeName = "Bookmark",
            Attributes = new Dictionary<string, FieldType>
            {
                ["position"] = FieldType.Long,
                ["username"] = FieldType.String,
                ["comment"] = FieldType.String,
                ["created"] = FieldType.DateTime,
                ["changed"] = FieldType.DateTime
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["entry"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            },
            RequiredAttributes = new[] { "position" }
        },
        ["bookmarks"] = new ElementDefinition
        {
            TypeName = "Bookmarks",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["bookmark"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Bookmark" }
            }
        },
        ["playQueue"] = new ElementDefinition
        {
            TypeName = "PlayQueue",
            Attributes = new Dictionary<string, FieldType>
            {
                ["username"] = FieldType.String,
                ["current"] = FieldType.String,
                ["position"] = FieldType.Long
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["entry"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            }
        },
        ["scanStatus"] = new ElementDefinition
        {
            TypeName = "ScanStatus",
            Attributes = new Dictionary<string, FieldType>
            {
                ["scanning"] = FieldType.Boolean,
                ["count"] = FieldType.Long,
                ["currentCount"] = FieldType.Long,
                ["totalCount"] = FieldType.Long,
                ["currentPercent"] = FieldType.Integer,
                ["totalPercent"] = FieldType.Integer
            },
            RequiredAttributes = Array.Empty<string>()
        },
        ["similarSongs2"] = new ElementDefinition
        {
            TypeName = "SimilarSongs2",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            }
        },
        ["topSongs"] = new ElementDefinition
        {
            TypeName = "TopSongs",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            }
        },
        ["songsByGenre"] = new ElementDefinition
        {
            TypeName = "Songs",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["song"] = new FieldDefinition { Type = FieldType.Array, ItemType = "Child" }
            }
        },
        ["Album"] = new ElementDefinition
        {
            TypeName = "AlbumInfo",
            Attributes = new Dictionary<string, FieldType>
            {
                ["title"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["artistId"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["notes"] = FieldType.String,
                ["musicBrainzId"] = FieldType.String,
                ["lastFmUrl"] = FieldType.String,
                ["smallImageUrl"] = FieldType.String,
                ["mediumImageUrl"] = FieldType.String,
                ["largeImageUrl"] = FieldType.String
            },
            RequiredAttributes = new[] { "title" }
        },
        ["InternetRadioStation"] = new ElementDefinition
        {
            TypeName = "InternetRadioStation",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["name"] = FieldType.String,
                ["streamUrl"] = FieldType.String,
                ["homePageUrl"] = FieldType.String
            },
            RequiredAttributes = new[] { "id", "name", "streamUrl" }
        },
        ["internetRadioStations"] = new ElementDefinition
        {
            TypeName = "InternetRadioStations",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["internetRadioStation"] = new FieldDefinition { Type = FieldType.Array, ItemType = "InternetRadioStation" }
            }
        },
        ["Podcasts"] = new ElementDefinition
        {
            TypeName = "Podcasts",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["channel"] = new FieldDefinition { Type = FieldType.Array, ItemType = "PodcastChannel" }
            }
        },
        ["PodcastChannel"] = new ElementDefinition
        {
            TypeName = "PodcastChannel",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["url"] = FieldType.String,
                ["title"] = FieldType.String,
                ["description"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["originalImageUrl"] = FieldType.String,
                ["link"] = FieldType.String,
                ["author"] = FieldType.String,
                ["description"] = FieldType.String,
                ["status"] = FieldType.String
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["episode"] = new FieldDefinition { Type = FieldType.Array, ItemType = "PodcastEpisode" }
            },
            RequiredAttributes = new[] { "id", "url" }
        },
        ["PodcastEpisode"] = new ElementDefinition
        {
            TypeName = "PodcastEpisode",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["channelId"] = FieldType.String,
                ["title"] = FieldType.String,
                ["description"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["link"] = FieldType.String,
                ["author"] = FieldType.String,
                ["duration"] = FieldType.Integer,
                ["publishDate"] = FieldType.DateTime,
                ["status"] = FieldType.String,
                ["fileSize"] = FieldType.Long
            },
            RequiredAttributes = new[] { "id", "channelId" }
        },
        ["Child"] = new ElementDefinition
        {
            TypeName = "Child",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["parent"] = FieldType.String,
                ["title"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["album"] = FieldType.String,
                ["genre"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["duration"] = FieldType.Integer,
                ["bitRate"] = FieldType.Integer,
                ["path"] = FieldType.String,
                ["fileSize"] = FieldType.Long,
                ["isDir"] = FieldType.Boolean,
                ["albumId"] = FieldType.String,
                ["artistId"] = FieldType.String,
                ["year"] = FieldType.Integer,
                ["track"] = FieldType.Integer,
                ["discNumber"] = FieldType.Integer,
                ["created"] = FieldType.DateTime,
                ["releaseDate"] = FieldType.DateTime,
                ["genreId"] = FieldType.String,
                ["crc32"] = FieldType.String,
                ["suffix"] = FieldType.String,
                ["contentType"] = FieldType.String,
                ["size"] = FieldType.Long
            },
            RequiredAttributes = new[] { "id" }
        },
        ["jukeboxStatus"] = new ElementDefinition
        {
            TypeName = "JukeboxStatus",
            Attributes = new Dictionary<string, FieldType>
            {
                ["currentIndex"] = FieldType.Integer,
                ["playing"] = FieldType.Boolean,
                ["gain"] = FieldType.Float,
                ["position"] = FieldType.Long
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["entry"] = new FieldDefinition { Type = FieldType.Array, ItemType = "JukeboxEntry" }
            },
            RequiredAttributes = new[] { "currentIndex", "playing" }
        },
        ["JukeboxEntry"] = new ElementDefinition
        {
            TypeName = "JukeboxEntry",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["title"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["album"] = FieldType.String,
                ["duration"] = FieldType.Integer
            },
            RequiredAttributes = new[] { "id" }
        },
        ["jukeboxPlaylist"] = new ElementDefinition
        {
            TypeName = "JukeboxPlaylist",
            Attributes = new Dictionary<string, FieldType>
            {
                ["changeCount"] = FieldType.Integer,
                ["currentIndex"] = FieldType.Integer,
                ["playing"] = FieldType.Boolean
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["entry"] = new FieldDefinition { Type = FieldType.Array, ItemType = "JukeboxEntry" }
            },
            RequiredAttributes = new[] { "changeCount" }
        },
        ["error"] = new ElementDefinition
        {
            TypeName = "Error",
            Attributes = new Dictionary<string, FieldType>
            {
                ["code"] = FieldType.Integer,
                ["message"] = FieldType.String
            },
            RequiredAttributes = new[] { "code", "message" }
        },
        ["podcasts"] = new ElementDefinition
        {
            TypeName = "Podcasts",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["channel"] = new FieldDefinition { Type = FieldType.Array, ItemType = "podcast" }
            }
        },
        ["podcast"] = new ElementDefinition
        {
            TypeName = "PodcastChannel",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["url"] = FieldType.String,
                ["title"] = FieldType.String,
                ["description"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["originalImageUrl"] = FieldType.String,
                ["link"] = FieldType.String,
                ["author"] = FieldType.String,
                ["status"] = FieldType.String
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["episode"] = new FieldDefinition { Type = FieldType.Array, ItemType = "episode" }
            },
            RequiredAttributes = new[] { "id", "url" }
        },
        ["episode"] = new ElementDefinition
        {
            TypeName = "PodcastEpisode",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["channelId"] = FieldType.String,
                ["title"] = FieldType.String,
                ["description"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["link"] = FieldType.String,
                ["author"] = FieldType.String,
                ["duration"] = FieldType.String,
                ["publishDate"] = FieldType.String,
                ["status"] = FieldType.String,
                ["fileSize"] = FieldType.Long
            },
            RequiredAttributes = new[] { "id", "channelId" }
        },
        ["newestPodcasts"] = new ElementDefinition
        {
            TypeName = "NewestPodcasts",
            Children = new Dictionary<string, FieldDefinition>
            {
                ["episode"] = new FieldDefinition { Type = FieldType.Array, ItemType = "episode" }
            }
        },
        ["jukeboxStatus"] = new ElementDefinition
        {
            TypeName = "JukeboxStatus",
            Attributes = new Dictionary<string, FieldType>
            {
                ["currentIndex"] = FieldType.Integer,
                ["playing"] = FieldType.Boolean,
                ["gain"] = FieldType.Float,
                ["position"] = FieldType.Long,
                ["maxVolume"] = FieldType.Integer
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["jukeboxPlaylist"] = new FieldDefinition { Type = FieldType.Single, ItemType = "jukeboxPlaylist" }
            },
            RequiredAttributes = new[] { "currentIndex", "playing" }
        },
        ["jukeboxPlaylist"] = new ElementDefinition
        {
            TypeName = "JukeboxPlaylist",
            Attributes = new Dictionary<string, FieldType>
            {
                ["changeCount"] = FieldType.Integer,
                ["currentIndex"] = FieldType.Integer,
                ["playing"] = FieldType.Boolean,
                ["username"] = FieldType.String,
                ["comment"] = FieldType.String,
                ["public"] = FieldType.Boolean,
                ["songCount"] = FieldType.Integer,
                ["duration"] = FieldType.Integer
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["entry"] = new FieldDefinition { Type = FieldType.Array, ItemType = "jukeboxEntry" }
            },
            RequiredAttributes = new[] { "changeCount" }
        },
        ["jukeboxEntry"] = new ElementDefinition
        {
            TypeName = "JukeboxEntry",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["parent"] = FieldType.String,
                ["title"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["album"] = FieldType.String,
                ["year"] = FieldType.Integer,
                ["genre"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["duration"] = FieldType.Integer,
                ["bitRate"] = FieldType.Integer,
                ["path"] = FieldType.String,
                ["transcodedContentType"] = FieldType.String,
                ["transcodedSuffix"] = FieldType.String,
                ["isDir"] = FieldType.Boolean,
                ["isVideo"] = FieldType.Boolean,
                ["type"] = FieldType.String
            },
            RequiredAttributes = new[] { "id" }
        },
        ["AlbumChild"] = new ElementDefinition
        {
            TypeName = "AlbumChild",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["name"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["artistId"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["year"] = FieldType.Integer,
                ["genre"] = FieldType.String,
                ["songCount"] = FieldType.Integer,
                ["duration"] = FieldType.Integer,
                ["created"] = FieldType.DateTime,
                ["albumArtist"] = FieldType.String,
                ["albumArtistId"] = FieldType.String,
                ["parent"] = FieldType.String
            },
            RequiredAttributes = new[] { "id", "name" }
        },
        ["LyricsVerse"] = new ElementDefinition
        {
            TypeName = "LyricsVerse",
            Attributes = new Dictionary<string, FieldType>
            {
                ["nr"] = FieldType.Integer,
                ["value"] = FieldType.String
            },
            RequiredAttributes = new[] { "value" }
        },
        ["NowPlayingEntry"] = new ElementDefinition
        {
            TypeName = "NowPlayingEntry",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["parent"] = FieldType.String,
                ["title"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["album"] = FieldType.String,
                ["genre"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["duration"] = FieldType.Integer,
                ["bitRate"] = FieldType.Integer,
                ["path"] = FieldType.String,
                ["fileSize"] = FieldType.Long,
                ["isDir"] = FieldType.Boolean,
                ["albumId"] = FieldType.String,
                ["artistId"] = FieldType.String,
                ["year"] = FieldType.Integer,
                ["track"] = FieldType.Integer,
                ["discNumber"] = FieldType.Integer,
                ["created"] = FieldType.DateTime,
                ["releaseDate"] = FieldType.DateTime,
                ["genreId"] = FieldType.String,
                ["crc32"] = FieldType.String,
                ["suffix"] = FieldType.String,
                ["contentType"] = FieldType.String,
                ["size"] = FieldType.Long,
                ["username"] = FieldType.String,
                ["playerName"] = FieldType.String,
                ["minutesAgo"] = FieldType.Integer,
                ["secondsAgo"] = FieldType.Integer
            },
            RequiredAttributes = new[] { "id", "title" }
        },
        ["Share"] = new ElementDefinition
        {
            TypeName = "Share",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.Integer,
                ["url"] = FieldType.String,
                ["description"] = FieldType.String,
                ["username"] = FieldType.String,
                ["created"] = FieldType.DateTime,
                ["expires"] = FieldType.DateTime,
                ["lastVisited"] = FieldType.DateTime,
                ["visitCount"] = FieldType.Integer
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["entry"] = new FieldDefinition { Type = FieldType.Array, ItemType = "child" }
            },
            RequiredAttributes = new[] { "id", "url" }
        },
        ["Bookmark"] = new ElementDefinition
        {
            TypeName = "Bookmark",
            Attributes = new Dictionary<string, FieldType>
            {
                ["position"] = FieldType.Long,
                ["username"] = FieldType.String,
                ["comment"] = FieldType.String,
                ["created"] = FieldType.DateTime,
                ["changed"] = FieldType.DateTime
            },
            Children = new Dictionary<string, FieldDefinition>
            {
                ["entry"] = new FieldDefinition { Type = FieldType.Array, ItemType = "child" }
            },
            RequiredAttributes = new[] { "position" }
        },
        ["child"] = new ElementDefinition
        {
            TypeName = "Child",
            Attributes = new Dictionary<string, FieldType>
            {
                ["id"] = FieldType.String,
                ["parent"] = FieldType.String,
                ["title"] = FieldType.String,
                ["artist"] = FieldType.String,
                ["album"] = FieldType.String,
                ["genre"] = FieldType.String,
                ["coverArt"] = FieldType.String,
                ["duration"] = FieldType.Integer,
                ["bitRate"] = FieldType.Integer,
                ["path"] = FieldType.String,
                ["fileSize"] = FieldType.Long,
                ["isDir"] = FieldType.Boolean,
                ["albumId"] = FieldType.String,
                ["artistId"] = FieldType.String,
                ["year"] = FieldType.Integer,
                ["track"] = FieldType.Integer,
                ["discNumber"] = FieldType.Integer,
                ["created"] = FieldType.DateTime,
                ["releaseDate"] = FieldType.DateTime,
                ["genreId"] = FieldType.String,
                ["crc32"] = FieldType.String,
                ["suffix"] = FieldType.String,
                ["contentType"] = FieldType.String,
                ["size"] = FieldType.Long
            },
            RequiredAttributes = new[] { "id" }
        }
    };

    public static List<string> ValidateResponseElement(string elementName, JsonNode? element)
    {
        var errors = new List<string>();

        if (element == null)
        {
            errors.Add($"Element '{elementName}' is null");
            return errors;
        }

        if (!ElementDefinitions.TryGetValue(elementName, out var definition))
        {
            errors.Add($"Unknown element type '{elementName}' - cannot validate against XSD");
            return errors;
        }

        if (definition.Attributes != null)
        {
            foreach (var attr in definition.Attributes)
            {
                var attrValue = element[attr.Key];
                if (attrValue != null)
                {
                    var validationError = ValidateFieldType(elementName, attr.Key, attrValue, attr.Value);
                    if (validationError != null)
                    {
                        errors.Add(validationError);
                    }
                }
            }
        }

        if (definition.RequiredAttributes != null)
        {
            foreach (var requiredAttr in definition.RequiredAttributes)
            {
                if (element[requiredAttr] == null)
                {
                    errors.Add($"Element '{elementName}' is missing required attribute '{requiredAttr}'");
                }
            }
        }

        if (definition.Children != null)
        {
            foreach (var child in definition.Children)
            {
                var childValue = element[child.Key];
                if (childValue != null)
                {
                    if (child.Value.Type == FieldType.Array)
                    {
                        if (childValue is JsonArray arr)
                        {
                            foreach (var item in arr)
                            {
                                var itemErrors = ValidateResponseElement(child.Value.ItemType ?? child.Key, item);
                                errors.AddRange(itemErrors.Select(e => $"  {e}"));
                            }
                        }
                        else
                        {
                            errors.Add($"Element '{elementName}/{child.Key}' should be an array");
                        }
                    }
                    else if (child.Value.Type == FieldType.Single)
                    {
                        var itemErrors = ValidateResponseElement(child.Value.ItemType ?? child.Key, childValue);
                        errors.AddRange(itemErrors.Select(e => $"  {e}"));
                    }
                }
            }
        }

        return errors;
    }

    private static string? ValidateFieldType(string elementName, string fieldName, JsonNode fieldValue, FieldType expectedType)
    {
        switch (expectedType)
        {
            case FieldType.Integer:
                if (fieldValue.GetValueKind() != JsonValueKind.Number)
                    return $"Field '{elementName}/{fieldName}' should be an integer";
                break;
            case FieldType.Long:
                if (fieldValue.GetValueKind() != JsonValueKind.Number)
                    return $"Field '{elementName}/{fieldName}' should be a long/integer";
                break;
            case FieldType.Boolean:
                if (fieldValue.GetValueKind() != JsonValueKind.True &&
                    fieldValue.GetValueKind() != JsonValueKind.False)
                    return $"Field '{elementName}/{fieldName}' should be a boolean";
                break;
            case FieldType.DateTime:
                var dateStr = fieldValue.ToString();
                if (!DateTime.TryParse(dateStr, out _))
                    return $"Field '{elementName}/{fieldName}' should be a valid dateTime ('{dateStr}' is invalid)";
                break;
            case FieldType.Float:
                if (fieldValue.GetValueKind() != JsonValueKind.Number)
                    return $"Field '{elementName}/{fieldName}' should be a float/number";
                break;
        }
        return null;
    }
}

public class ElementDefinition
{
    public string TypeName { get; set; } = "";
    public Dictionary<string, FieldType>? Attributes { get; set; }
    public string[] RequiredAttributes { get; set; } = Array.Empty<string>();
    public Dictionary<string, FieldDefinition>? Children { get; set; }
}

public class FieldDefinition
{
    public FieldType Type { get; set; }
    public string? ItemType { get; set; }
}

public enum FieldType
{
    String,
    Integer,
    Long,
    Boolean,
    DateTime,
    Array,
    Float,
    Single
}
