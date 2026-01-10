using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class ChangePodcastEpisodePublishDateToInstant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1803,
                columns: new[] { "Comment", "Value" },
                values: new object[] { "Maximum number of HTTP redirects to follow for podcast feeds. Podcast CDNs often use multiple analytics redirects, so 10 is recommended.", "10" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1803,
                columns: new[] { "Comment", "Value" },
                values: new object[] { "Maximum number of HTTP redirects to follow for podcast feeds.", "5" });
        }
    }
}
