using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddPodcastPhase2Features : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "QueuedAt",
                table: "PodcastEpisodes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxDownloadedEpisodes",
                table: "PodcastChannels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MaxStorageBytes",
                table: "PodcastChannels",
                type: "bigint",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1813, new Guid("153a12d4-77b4-ccc3-1584-f3685d6c9e2e"), 15, "Keep only the last N downloaded episodes per channel. 0 to disable this policy.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.retention.keepLastNEpisodes", null, null, 0, null, "0" },
                    { 1814, new Guid("3da9402e-9566-c883-66e5-d232de677199"), 15, "Delete downloaded episodes after they have been played. false to disable.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.retention.keepUnplayedOnly", null, null, 0, null, "false" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1813);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1814);

            migrationBuilder.DropColumn(
                name: "QueuedAt",
                table: "PodcastEpisodes");

            migrationBuilder.DropColumn(
                name: "MaxDownloadedEpisodes",
                table: "PodcastChannels");

            migrationBuilder.DropColumn(
                name: "MaxStorageBytes",
                table: "PodcastChannels");
        }
    }
}
