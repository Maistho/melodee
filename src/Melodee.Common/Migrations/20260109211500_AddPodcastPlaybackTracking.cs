using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddPodcastPlaybackTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PodcastEpisodeBookmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PodcastEpisodeId = table.Column<int>(type: "integer", nullable: false),
                    PositionSeconds = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastEpisodeBookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodcastEpisodeBookmarks_PodcastEpisodes_PodcastEpisodeId",
                        column: x => x.PodcastEpisodeId,
                        principalTable: "PodcastEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PodcastEpisodeBookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPodcastEpisodePlayHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PodcastEpisodeId = table.Column<int>(type: "integer", nullable: false),
                    PlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    Client = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ByUserAgent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SecondsPlayed = table.Column<int>(type: "integer", nullable: true),
                    Source = table.Column<short>(type: "smallint", nullable: false),
                    IsNowPlaying = table.Column<bool>(type: "boolean", nullable: false),
                    LastHeartbeatAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPodcastEpisodePlayHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPodcastEpisodePlayHistories_PodcastEpisodes_PodcastEpis~",
                        column: x => x.PodcastEpisodeId,
                        principalTable: "PodcastEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPodcastEpisodePlayHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEpisodeBookmarks_PodcastEpisodeId",
                table: "PodcastEpisodeBookmarks",
                column: "PodcastEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEpisodeBookmarks_UserId_PodcastEpisodeId",
                table: "PodcastEpisodeBookmarks",
                columns: new[] { "UserId", "PodcastEpisodeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPodcastEpisodePlayHistories_PodcastEpisodeId_PlayedAt",
                table: "UserPodcastEpisodePlayHistories",
                columns: new[] { "PodcastEpisodeId", "PlayedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPodcastEpisodePlayHistories_UserId_PodcastEpisodeId_Pla~",
                table: "UserPodcastEpisodePlayHistories",
                columns: new[] { "UserId", "PodcastEpisodeId", "PlayedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PodcastEpisodeBookmarks");

            migrationBuilder.DropTable(
                name: "UserPodcastEpisodePlayHistories");
        }
    }
}
