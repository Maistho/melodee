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
            // Drop index if exists (safe for re-running)
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_PartyAuditEvents_CreatedAt"";
            ");

            // Drop column if exists
            migrationBuilder.Sql(@"
                ALTER TABLE ""PartyAuditEvents"" DROP COLUMN IF EXISTS ""Message"";
            ");

            // Rename column if it exists and new name doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PartyAuditEvents' AND column_name = 'MetadataJson')
                       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PartyAuditEvents' AND column_name = 'PayloadJson') THEN
                        ALTER TABLE ""PartyAuditEvents"" RENAME COLUMN ""MetadataJson"" TO ""PayloadJson"";
                    END IF;
                END $$;
            ");

            // Add column if not exists
            migrationBuilder.Sql(@"
                ALTER TABLE ""PartySessions"" ADD COLUMN IF NOT EXISTS ""IsQueueLocked"" boolean NOT NULL DEFAULT false;
            ");

            // Alter column (UserId to not nullable) - this is idempotent
            migrationBuilder.Sql(@"
                ALTER TABLE ""PartyAuditEvents"" ALTER COLUMN ""UserId"" SET NOT NULL;
                ALTER TABLE ""PartyAuditEvents"" ALTER COLUMN ""UserId"" SET DEFAULT 0;
            ");

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1900, new Guid("541a397c-740c-8b9d-f1ed-5f990cab92a1"), 16, "Enable Jukebox support for server-side playback.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "jukebox.enabled", null, null, 0, null, "false" },
                    { 1901, new Guid("4c886427-ffc2-d277-5950-6cf4b880b7be"), 16, "The type of backend to use for jukebox playback (e.g., 'mpv'). Leave empty for no backend.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "jukebox.backendType", null, null, 0, null, "" }
                });

            // Add foreign keys only if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_PartyAuditEvents_PartySessions_PartySessionId') THEN
                        ALTER TABLE ""PartyAuditEvents"" ADD CONSTRAINT ""FK_PartyAuditEvents_PartySessions_PartySessionId"" 
                            FOREIGN KEY (""PartySessionId"") REFERENCES ""PartySessions"" (""Id"") ON DELETE CASCADE;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_PartyAuditEvents_Users_UserId') THEN
                        ALTER TABLE ""PartyAuditEvents"" ADD CONSTRAINT ""FK_PartyAuditEvents_Users_UserId"" 
                            FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE;
                    END IF;
                END $$;
            ");
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

