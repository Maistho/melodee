using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_EndpointRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEndpointOffline",
                table: "PartySessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PartyAuditEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartySessionId = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyAuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyAuditEvents_PartySessions_PartySessionId",
                        column: x => x.PartySessionId,
                        principalTable: "PartySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartyAuditEvents_ApiKey",
                table: "PartyAuditEvents",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyAuditEvents_CreatedAt",
                table: "PartyAuditEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartyAuditEvents_PartySessionId",
                table: "PartyAuditEvents",
                column: "PartySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyAuditEvents_UserId",
                table: "PartyAuditEvents",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartyAuditEvents");

            migrationBuilder.DropColumn(
                name: "IsEndpointOffline",
                table: "PartySessions");
        }
    }
}
