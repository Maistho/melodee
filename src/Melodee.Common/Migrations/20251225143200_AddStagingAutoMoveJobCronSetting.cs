using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddStagingAutoMoveJobCronSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { new Guid("8ac2b01f-d88b-4a48-9a60-75dbf213ad94"), 14, "Cron expression for staging auto-move job. Moves 'Ok' albums to storage. Default '0 */15 * * * ?' runs every 15 min. Also triggered after inbound processing.", NodaTime.Instant.FromUnixTimeTicks(17666731196080941L), null, false, "jobs.stagingAutoMove.cronExpression", null, null, 0, null, "0 */15 * * * ?" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "ApiKey",
                keyValue: new Guid("8ac2b01f-d88b-4a48-9a60-75dbf213ad94"));
        }
    }
}
