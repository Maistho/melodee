using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddJellyfinAccessToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JellyfinAccessTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TokenPrefixHash = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TokenSalt = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Client = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Device = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DeviceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Version = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JellyfinAccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JellyfinAccessTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1700, new Guid("226cfbc6-3866-fa17-7729-23849a7b8077"), null, "Enable Jellyfin API compatibility", NodaTime.Instant.FromUnixTimeTicks(0L), "When enabled, Melodee exposes Jellyfin-compatible endpoints for third-party music players", false, "jellyfin.enabled", null, null, 0, null, "true" },
                    { 1701, new Guid("eefa4040-71d4-b7b0-4218-52b5aa1c7408"), null, "Internal route prefix for Jellyfin API", NodaTime.Instant.FromUnixTimeTicks(0L), "The internal route prefix used for Jellyfin API endpoints (default: /api/jf)", false, "jellyfin.routePrefix", null, null, 0, null, "/api/jf" },
                    { 1702, new Guid("57d8a083-6ad7-9d6f-a31f-8b4f94e7a2a0"), null, "Jellyfin token expiry time in hours", NodaTime.Instant.FromUnixTimeTicks(0L), "How long Jellyfin access tokens remain valid (default: 168 hours / 7 days)", false, "jellyfin.token.expiresAfterHours", null, null, 0, null, "168" },
                    { 1703, new Guid("1696717a-dbe7-3278-52c1-bc43a5c7ed86"), null, "Maximum active Jellyfin tokens per user", NodaTime.Instant.FromUnixTimeTicks(0L), "The maximum number of active Jellyfin tokens allowed per user (default: 10)", false, "jellyfin.token.maxActivePerUser", null, null, 0, null, "10" },
                    { 1704, new Guid("732d29c7-1df6-4084-b126-f485463a10a4"), null, "Allow legacy Emby/MediaBrowser headers", NodaTime.Instant.FromUnixTimeTicks(0L), "Allow X-Emby-* and X-MediaBrowser-* headers for authentication (default: true)", false, "jellyfin.token.allowLegacyHeaders", null, null, 0, null, "true" },
                    { 1705, new Guid("57ef8277-a41c-a3e3-d68b-3e6c16a98728"), null, "Secret pepper for Jellyfin token hashing", NodaTime.Instant.FromUnixTimeTicks(0L), "Server-side secret used in token hash computation. Change this value in production for added security.", false, "jellyfin.token.pepper", null, null, 0, null, "ChangeThisPepperInProduction" },
                    { 1706, new Guid("191427dc-3a4b-e304-fe21-9457435456d7"), null, "API requests allowed per period", NodaTime.Instant.FromUnixTimeTicks(0L), "Maximum number of Jellyfin API requests allowed per rate limit period (default: 200)", false, "jellyfin.rateLimit.apiRequestsPerPeriod", null, null, 0, null, "200" },
                    { 1707, new Guid("e10e7d3e-d4e8-a507-7a8e-ff526828ddd1"), null, "Rate limit period in seconds", NodaTime.Instant.FromUnixTimeTicks(0L), "Duration of the rate limit period in seconds (default: 60)", false, "jellyfin.rateLimit.apiPeriodSeconds", null, null, 0, null, "60" },
                    { 1708, new Guid("96e4d8c5-a98c-ecd1-755a-eaccd69eaa20"), null, "Concurrent streams per user", NodaTime.Instant.FromUnixTimeTicks(0L), "Maximum number of concurrent audio streams allowed per user (default: 2)", false, "jellyfin.rateLimit.streamConcurrentPerUser", null, null, 0, null, "2" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_JellyfinAccessTokens_TokenHash",
                table: "JellyfinAccessTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JellyfinAccessTokens_TokenPrefixHash",
                table: "JellyfinAccessTokens",
                column: "TokenPrefixHash");

            migrationBuilder.CreateIndex(
                name: "IX_JellyfinAccessTokens_UserId",
                table: "JellyfinAccessTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JellyfinAccessTokens_UserId_ExpiresAt_RevokedAt",
                table: "JellyfinAccessTokens",
                columns: new[] { "UserId", "ExpiresAt", "RevokedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JellyfinAccessTokens");

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1700);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1701);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1702);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1703);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1704);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1705);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1706);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1707);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1708);
        }
    }
}
