using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePodcastOpSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { 1812, new Guid("737e544b-7490-d53e-a092-3fd6e2b629b4"), 15, "Maximum total storage in bytes for all podcasts per user. 0 for unlimited.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "podcast.quota.maxBytesPerUser", null, null, 0, null, "5368709120" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1812);
        }
    }
}
