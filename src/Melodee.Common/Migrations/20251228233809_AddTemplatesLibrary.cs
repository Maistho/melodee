using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplatesLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "AlbumCount", "ApiKey", "ArtistCount", "CreatedAt", "Description", "IsLocked", "LastScanAt", "LastUpdatedAt", "Name", "Notes", "Path", "SongCount", "SortOrder", "Tags", "Type" },
                values: new object[] { 10, null, new Guid("62453b56-402b-8f9e-073b-e2d31e9f7cf9"), null, NodaTime.Instant.FromUnixTimeTicks(0L), "Library where email templates are stored, organized by language code.", false, null, null, "Email Templates", null, "/storage/templates/", null, 0, null, 7 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
