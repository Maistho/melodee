using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylistImportModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlaylistUploadedFileId",
                table: "Playlists",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "SourceType",
                table: "Playlists",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "PlaylistUploadedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    FileData = table.Column<byte[]>(type: "bytea", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistUploadedFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistUploadedFiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistUploadedFileItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlaylistUploadedFileId = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    RawReference = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    NormalizedReference = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    HintsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LastAttemptUtc = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistUploadedFileItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistUploadedFileItems_PlaylistUploadedFiles_PlaylistUpl~",
                        column: x => x.PlaylistUploadedFileId,
                        principalTable: "PlaylistUploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistUploadedFileItems_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                column: "Path",
                value: "/app/inbound/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                column: "Path",
                value: "/app/staging/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                column: "Path",
                value: "/app/storage/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                column: "Path",
                value: "/app/user-images/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                column: "Path",
                value: "/app/playlists/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 6,
                column: "Path",
                value: "/app/templates/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 7,
                column: "Path",
                value: "/app/podcasts/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 8,
                column: "Path",
                value: "/app/themes/");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_PlaylistUploadedFileId",
                table: "Playlists",
                column: "PlaylistUploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistUploadedFileItems_PlaylistUploadedFileId_SortOrder",
                table: "PlaylistUploadedFileItems",
                columns: new[] { "PlaylistUploadedFileId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistUploadedFileItems_SongId",
                table: "PlaylistUploadedFileItems",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistUploadedFiles_ApiKey",
                table: "PlaylistUploadedFiles",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistUploadedFiles_UserId",
                table: "PlaylistUploadedFiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlists_PlaylistUploadedFiles_PlaylistUploadedFileId",
                table: "Playlists",
                column: "PlaylistUploadedFileId",
                principalTable: "PlaylistUploadedFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlists_PlaylistUploadedFiles_PlaylistUploadedFileId",
                table: "Playlists");

            migrationBuilder.DropTable(
                name: "PlaylistUploadedFileItems");

            migrationBuilder.DropTable(
                name: "PlaylistUploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_PlaylistUploadedFileId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "PlaylistUploadedFileId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "Playlists");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                column: "Path",
                value: "/storage/inbound/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                column: "Path",
                value: "/storage/staging/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                column: "Path",
                value: "/storage/library/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                column: "Path",
                value: "/storage/images/users/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                column: "Path",
                value: "/storage/playlists/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 6,
                column: "Path",
                value: "/storage/templates/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 7,
                column: "Path",
                value: "/storage/podcasts/");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 8,
                column: "Path",
                value: "/storage/themes/");
        }
    }
}
