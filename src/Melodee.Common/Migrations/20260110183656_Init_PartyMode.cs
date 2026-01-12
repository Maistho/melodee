using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class Init_PartyMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartySessionEndpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerUserId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CapabilitiesJson = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true),
                    LastSeenAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    IsShared = table.Column<bool>(type: "boolean", nullable: false),
                    Room = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_PartySessionEndpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartySessionEndpoints_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PartySessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OwnerUserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    JoinCodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ActiveEndpointId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActiveEndpointId1 = table.Column<int>(type: "integer", nullable: true),
                    QueueRevision = table.Column<long>(type: "bigint", nullable: false),
                    PlaybackRevision = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_PartySessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartySessions_PartySessionEndpoints_ActiveEndpointId1",
                        column: x => x.ActiveEndpointId1,
                        principalTable: "PartySessionEndpoints",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartySessions_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartyQueueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartySessionId = table.Column<int>(type: "integer", nullable: false),
                    SongApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    EnqueuedByUserId = table.Column<int>(type: "integer", nullable: false),
                    EnqueuedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Note = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyQueueItems", x => x.Id);
                    table.UniqueConstraint("AK_PartyQueueItems_ApiKey", x => x.ApiKey);
                    table.ForeignKey(
                        name: "FK_PartyQueueItems_PartySessions_PartySessionId",
                        column: x => x.PartySessionId,
                        principalTable: "PartySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartyQueueItems_Users_EnqueuedByUserId",
                        column: x => x.EnqueuedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartySessionParticipants",
                columns: table => new
                {
                    PartySessionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartySessionParticipants", x => new { x.PartySessionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_PartySessionParticipants_PartySessions_PartySessionId",
                        column: x => x.PartySessionId,
                        principalTable: "PartySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartySessionParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartyPlaybackStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartySessionId = table.Column<int>(type: "integer", nullable: false),
                    CurrentQueueItemApiKey = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionSeconds = table.Column<double>(type: "double precision", nullable: false),
                    IsPlaying = table.Column<bool>(type: "boolean", nullable: false),
                    Volume = table.Column<double>(type: "double precision", nullable: true),
                    LastHeartbeatAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_PartyPlaybackStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyPlaybackStates_PartyQueueItems_CurrentQueueItemApiKey",
                        column: x => x.CurrentQueueItemApiKey,
                        principalTable: "PartyQueueItems",
                        principalColumn: "ApiKey",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PartyPlaybackStates_PartySessions_PartySessionId",
                        column: x => x.PartySessionId,
                        principalTable: "PartySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartyPlaybackStates_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartyPlaybackStates_ApiKey",
                table: "PartyPlaybackStates",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyPlaybackStates_CurrentQueueItemApiKey",
                table: "PartyPlaybackStates",
                column: "CurrentQueueItemApiKey");

            migrationBuilder.CreateIndex(
                name: "IX_PartyPlaybackStates_IsPlaying",
                table: "PartyPlaybackStates",
                column: "IsPlaying");

            migrationBuilder.CreateIndex(
                name: "IX_PartyPlaybackStates_LastHeartbeatAt",
                table: "PartyPlaybackStates",
                column: "LastHeartbeatAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartyPlaybackStates_PartySessionId",
                table: "PartyPlaybackStates",
                column: "PartySessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyPlaybackStates_UpdatedByUserId",
                table: "PartyPlaybackStates",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyQueueItems_ApiKey",
                table: "PartyQueueItems",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyQueueItems_EnqueuedAt",
                table: "PartyQueueItems",
                column: "EnqueuedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartyQueueItems_EnqueuedByUserId",
                table: "PartyQueueItems",
                column: "EnqueuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyQueueItems_PartySessionId_SortOrder",
                table: "PartyQueueItems",
                columns: new[] { "PartySessionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PartyQueueItems_SongApiKey",
                table: "PartyQueueItems",
                column: "SongApiKey");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessionEndpoints_ApiKey",
                table: "PartySessionEndpoints",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartySessionEndpoints_IsShared",
                table: "PartySessionEndpoints",
                column: "IsShared");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessionEndpoints_LastSeenAt",
                table: "PartySessionEndpoints",
                column: "LastSeenAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessionEndpoints_OwnerUserId",
                table: "PartySessionEndpoints",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessionEndpoints_Room",
                table: "PartySessionEndpoints",
                column: "Room");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessionEndpoints_Type",
                table: "PartySessionEndpoints",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessionParticipants_IsBanned",
                table: "PartySessionParticipants",
                column: "IsBanned");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessionParticipants_PartySessionId_UserId",
                table: "PartySessionParticipants",
                columns: new[] { "PartySessionId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartySessionParticipants_Role",
                table: "PartySessionParticipants",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessionParticipants_UserId",
                table: "PartySessionParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessions_ActiveEndpointId",
                table: "PartySessions",
                column: "ActiveEndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessions_ActiveEndpointId1",
                table: "PartySessions",
                column: "ActiveEndpointId1");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessions_ApiKey",
                table: "PartySessions",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartySessions_OwnerUserId",
                table: "PartySessions",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartySessions_Status",
                table: "PartySessions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartyPlaybackStates");

            migrationBuilder.DropTable(
                name: "PartySessionParticipants");

            migrationBuilder.DropTable(
                name: "PartyQueueItems");

            migrationBuilder.DropTable(
                name: "PartySessions");

            migrationBuilder.DropTable(
                name: "PartySessionEndpoints");
        }
    }
}
