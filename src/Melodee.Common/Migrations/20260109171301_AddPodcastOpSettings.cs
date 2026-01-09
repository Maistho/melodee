using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddPodcastOpSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1809, new Guid("908afec1-3a49-5e62-26f5-d6977ef6b00c"), 15, "Number of days to keep downloaded episodes. 0 to disable retention.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.retention.downloadedEpisodesInDays", null, null, 0, null, "0" },
                    { 1810, new Guid("6f86302a-1d6d-b574-c77a-b6cfbefb5e0a"), 15, "Threshold in minutes to consider a downloading episode as stuck.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.recovery.stuckDownloadThresholdMinutes", null, null, 0, null, "60" },
                    { 1811, new Guid("8d257a4b-b566-e0af-1044-9658d5ac27ea"), 15, "Threshold in hours to consider a temporary file orphaned.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.recovery.orphanedUsageThresholdHours", null, null, 0, null, "12" },
                    { 1852, new Guid("3b2df55c-cd9c-a51b-2c4c-8f566bf7b6d8"), 14, "Cron expression to run the podcast cleanup job, set empty to disable. Default of '0 0 2 * * ?' runs daily at 2 AM.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "jobs.podcastCleanup.cronExpression", null, null, 0, null, "0 0 2 * * ?" },
                    { 1853, new Guid("17b25fcb-6a54-291d-5927-28ade4b15a93"), 14, "Cron expression to run the podcast recovery job, set empty to disable. Default of '0 */30 * ? * *' runs every 30 minutes.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "jobs.podcastRecovery.cronExpression", null, null, 0, null, "0 */30 * ? * *" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1809);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1810);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1811);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1852);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1853);
        }
    }
}
