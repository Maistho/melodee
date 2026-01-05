using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class FixLibrarySeedDataCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update seeded Library records that have CreatedAt set to Unix epoch (1970-01-01)
            // to use the current timestamp instead
            migrationBuilder.Sql(
                """
                UPDATE "Libraries"
                SET "CreatedAt" = NOW()
                WHERE "CreatedAt" = '1970-01-01 00:00:00+00'
                """);

            // Also update seeded Setting records
            migrationBuilder.Sql(
                """
                UPDATE "Settings"
                SET "CreatedAt" = NOW()
                WHERE "CreatedAt" = '1970-01-01 00:00:00+00'
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback needed - the original epoch timestamps were not meaningful
        }
    }
}
