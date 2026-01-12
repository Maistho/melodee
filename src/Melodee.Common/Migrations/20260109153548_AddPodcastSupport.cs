using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddPodcastSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PodcastChannels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FeedUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true),
                    SiteUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CoverArtLocalPath = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Etag = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSyncAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    LastSyncAttemptAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    LastSyncError = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ConsecutiveFailureCount = table.Column<int>(type: "integer", nullable: false),
                    NextSyncAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmartPlaylists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MqlQuery = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastResultCount = table.Column<int>(type: "integer", nullable: false),
                    LastEvaluatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    NormalizedQuery = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartPlaylists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmartPlaylists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PodcastEpisodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PodcastChannelId = table.Column<int>(type: "integer", nullable: false),
                    Guid = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true),
                    PublishDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EnclosureUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EnclosureLength = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EpisodeKey = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DownloadStatus = table.Column<int>(type: "integer", nullable: false),
                    DownloadError = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LocalPath = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LocalFileSize = table.Column<long>(type: "bigint", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastEpisodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodcastEpisodes_PodcastChannels_PodcastChannelId",
                        column: x => x.PodcastChannelId,
                        principalTable: "PodcastChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "Library where templates are stored, organized by language code.");

            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "AlbumCount", "ApiKey", "ArtistCount", "CreatedAt", "Description", "IsLocked", "LastScanAt", "LastUpdatedAt", "Name", "Notes", "Path", "SongCount", "SortOrder", "Tags", "Type" },
                values: new object[] { 7, null, new Guid("01d52713-b3cf-48fa-f085-7704baee6dc5"), null, NodaTime.Instant.FromUnixTimeTicks(0L), "Library where podcast media files are stored.", false, null, null, "Podcasts", null, "/storage/podcasts/", null, 0, null, 8 });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1800, new Guid("8ee4c50d-9a7a-a4ef-66f1-74614a24313e"), 15, "Enable podcast support.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.enabled", null, null, 0, null, "true" },
                    { 1801, new Guid("c3d99d92-ab8d-bdca-ab08-3cc6ea2d2860"), 15, "Allow HTTP (non-secure) URLs for podcast feeds.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.http.allowHttp", null, null, 0, null, "false" },
                    { 1802, new Guid("93b35ab7-14d0-0814-0d66-fe040e3ae4b8"), 15, "Timeout in seconds for HTTP requests to podcast feeds.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.http.timeoutSeconds", null, null, 0, null, "30" },
                    { 1803, new Guid("6b35ba44-07ac-645d-b2a3-9cadaa60ff3d"), 15, "Maximum number of HTTP redirects to follow for podcast feeds.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.http.maxRedirects", null, null, 0, null, "5" },
                    { 1804, new Guid("13168117-a286-23b5-5858-9f91485c6432"), 15, "Maximum size in bytes for podcast feed responses.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.http.maxFeedBytes", null, null, 0, null, "10485760" },
                    { 1805, new Guid("1fceaf81-79eb-433c-de79-eabe193c46f8"), 15, "Maximum number of episodes to store per podcast channel.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.refresh.maxItemsPerChannel", null, null, 0, null, "500" },
                    { 1806, new Guid("525bb5dc-989c-5154-0c7e-7f4b336032e3"), 15, "Maximum concurrent podcast episode downloads (global).", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.download.maxConcurrent.global", null, null, 0, null, "2" },
                    { 1807, new Guid("380ed177-9320-92a0-5a93-48bdcc040d35"), 15, "Maximum concurrent podcast episode downloads per user.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.download.maxConcurrent.perUser", null, null, 0, null, "1" },
                    { 1808, new Guid("2d5158e7-495e-44a6-e06a-b5f1359f8ea2"), 15, "Maximum size in bytes for podcast episode downloads.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.download.maxEnclosureBytes", null, null, 0, null, "2147483648" },
                    { 1850, new Guid("dc79ceff-cd68-f412-8f99-7529615cb3e8"), 14, "Cron expression to run the podcast refresh job, set empty to disable. Default of '0 */15 * ? * *' runs every 15 minutes.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "jobs.podcastRefresh.cronExpression", null, null, 0, null, "0 */15 * ? * *" },
                    { 1851, new Guid("d29b11cc-d892-271a-9e2a-5eeacb795e39"), 14, "Cron expression to run the podcast download job, set empty to disable. Default of '0 */5 * ? * *' runs every 5 minutes.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "jobs.podcastDownload.cronExpression", null, null, 0, null, "0 */5 * ? * *" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PodcastChannels_ApiKey",
                table: "PodcastChannels",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodcastChannels_IsDeleted",
                table: "PodcastChannels",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastChannels_NextSyncAt",
                table: "PodcastChannels",
                column: "NextSyncAt");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastChannels_UserId_FeedUrl",
                table: "PodcastChannels",
                columns: new[] { "UserId", "FeedUrl" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEpisodes_ApiKey",
                table: "PodcastEpisodes",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEpisodes_PodcastChannelId_DownloadStatus",
                table: "PodcastEpisodes",
                columns: new[] { "PodcastChannelId", "DownloadStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEpisodes_PodcastChannelId_EpisodeKey",
                table: "PodcastEpisodes",
                columns: new[] { "PodcastChannelId", "EpisodeKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEpisodes_PodcastChannelId_PublishDate",
                table: "PodcastEpisodes",
                columns: new[] { "PodcastChannelId", "PublishDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SmartPlaylists_ApiKey",
                table: "SmartPlaylists",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmartPlaylists_IsPublic",
                table: "SmartPlaylists",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_SmartPlaylists_UserId",
                table: "SmartPlaylists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmartPlaylists_UserId_Name",
                table: "SmartPlaylists",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PodcastEpisodes");

            migrationBuilder.DropTable(
                name: "SmartPlaylists");

            migrationBuilder.DropTable(
                name: "PodcastChannels");

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1709);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1710);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1711);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1800);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1801);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1802);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1803);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1804);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1805);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1806);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1807);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1808);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1850);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1851);

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "Library where email templates are stored, organized by language code.");
        }
    }
}
