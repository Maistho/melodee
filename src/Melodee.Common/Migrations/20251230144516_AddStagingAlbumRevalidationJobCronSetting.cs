using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddStagingAlbumRevalidationJobCronSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { new Guid("b9f4a3e2-7c81-4d5f-8a2b-3e1f9c6d0a4b"), 14, "Cron expression for staging album revalidation job. Re-validates albums with invalid artists. Default '0 0 3 ? * SUN' runs weekly on Sunday at 3am.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "jobs.stagingAlbumRevalidation.cronExpression", null, null, 0, null, "0 0 3 ? * SUN" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "ApiKey",
                keyValue: new Guid("b9f4a3e2-7c81-4d5f-8a2b-3e1f9c6d0a4b"));
        }
    }
}
