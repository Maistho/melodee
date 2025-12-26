using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class FixRequestIndexDescending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_CreatedAt_Id",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_LastActivityAt_Id",
                table: "Requests");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CreatedAt_Id",
                table: "Requests",
                columns: new[] { "CreatedAt", "Id" },
                descending: new[] { true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_LastActivityAt_Id",
                table: "Requests",
                columns: new[] { "LastActivityAt", "Id" },
                descending: new[] { true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_CreatedAt_Id",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_LastActivityAt_Id",
                table: "Requests");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CreatedAt_Id",
                table: "Requests",
                columns: new[] { "CreatedAt", "Id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_LastActivityAt_Id",
                table: "Requests",
                columns: new[] { "LastActivityAt", "Id" },
                descending: new bool[0]);
        }
    }
}
