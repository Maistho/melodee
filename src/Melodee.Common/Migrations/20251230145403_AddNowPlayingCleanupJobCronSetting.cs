using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddNowPlayingCleanupJobCronSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { new Guid("c7d5e2f1-8a93-4b6e-9c1d-2f3a4b5c6d7e"), 14, "Cron expression for now playing cleanup job. Removes stale now playing records. Default '0 */5 * * * ?' runs every 5 minutes.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "jobs.nowPlayingCleanup.cronExpression", null, null, 0, null, "0 */5 * * * ?" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "ApiKey",
                keyValue: new Guid("c7d5e2f1-8a93-4b6e-9c1d-2f3a4b5c6d7e"));
        }
    }
}
