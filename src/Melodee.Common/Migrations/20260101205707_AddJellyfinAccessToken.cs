using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddJellyfinAccessToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JellyfinAccessTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TokenPrefixHash = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TokenSalt = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Client = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Device = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DeviceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Version = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JellyfinAccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JellyfinAccessTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JellyfinAccessTokens_TokenHash",
                table: "JellyfinAccessTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JellyfinAccessTokens_TokenPrefixHash",
                table: "JellyfinAccessTokens",
                column: "TokenPrefixHash");

            migrationBuilder.CreateIndex(
                name: "IX_JellyfinAccessTokens_UserId",
                table: "JellyfinAccessTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JellyfinAccessTokens_UserId_ExpiresAt_RevokedAt",
                table: "JellyfinAccessTokens",
                columns: new[] { "UserId", "ExpiresAt", "RevokedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JellyfinAccessTokens");
        }
    }
}
