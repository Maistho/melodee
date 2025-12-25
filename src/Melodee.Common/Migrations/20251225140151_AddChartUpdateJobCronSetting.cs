using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddChartUpdateJobCronSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { new Guid("5163b103-1991-47b1-b095-736b2ec7bb2b"), 14, "Cron expression to run the chart update job which links chart items to albums, set empty to disable. Default of '0 0 2 * * ?' will run every day at 02:00. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17666713099655591L), null, false, "jobs.chartUpdate.cronExpression", null, null, 0, null, "0 0 2 * * ?" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "ApiKey",
                keyValue: new Guid("5163b103-1991-47b1-b095-736b2ec7bb2b"));
        }
    }
}
