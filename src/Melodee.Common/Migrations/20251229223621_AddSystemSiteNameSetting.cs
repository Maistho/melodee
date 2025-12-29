using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSiteNameSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { 1103, new Guid("9468bf96-8fea-8dfb-c1a9-7b764c5178c6"), 11, "Name for this Melodee instance (used in emails and UI branding).", NodaTime.Instant.FromUnixTimeTicks(0L), "Customize the display name of your Melodee instance. Defaults to 'Melodee' if not set.", false, "system.siteName", null, null, 0, null, "Melodee" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1103);
        }
    }
}
