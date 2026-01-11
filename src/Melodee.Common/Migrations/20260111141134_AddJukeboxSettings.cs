using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddJukeboxSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PartyAuditEvents_CreatedAt",
                table: "PartyAuditEvents");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "PartyAuditEvents");

            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "PartyAuditEvents",
                newName: "PayloadJson");

            migrationBuilder.AddColumn<bool>(
                name: "IsQueueLocked",
                table: "PartySessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "PartyAuditEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1900, new Guid("541a397c-740c-8b9d-f1ed-5f990cab92a1"), 16, "Enable Jukebox support for server-side playback.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "jukebox.enabled", null, null, 0, null, "false" },
                    { 1901, new Guid("4c886427-ffc2-d277-5950-6cf4b880b7be"), 16, "The type of backend to use for jukebox playback (e.g., 'mpv'). Leave empty for no backend.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "jukebox.backendType", null, null, 0, null, "" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_PartyAuditEvents_PartySessions_PartySessionId",
                table: "PartyAuditEvents",
                column: "PartySessionId",
                principalTable: "PartySessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartyAuditEvents_Users_UserId",
                table: "PartyAuditEvents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyAuditEvents_PartySessions_PartySessionId",
                table: "PartyAuditEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_PartyAuditEvents_Users_UserId",
                table: "PartyAuditEvents");

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1900);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1901);

            migrationBuilder.DropColumn(
                name: "IsQueueLocked",
                table: "PartySessions");

            migrationBuilder.RenameColumn(
                name: "PayloadJson",
                table: "PartyAuditEvents",
                newName: "MetadataJson");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "PartyAuditEvents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "PartyAuditEvents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyAuditEvents_CreatedAt",
                table: "PartyAuditEvents",
                column: "CreatedAt");
        }
    }
}
