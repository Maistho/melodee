using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: false),
                    ArtistName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TargetArtistApiKey = table.Column<Guid>(type: "uuid", nullable: true),
                    AlbumTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TargetAlbumApiKey = table.Column<Guid>(type: "uuid", nullable: true),
                    SongTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TargetSongApiKey = table.Column<Guid>(type: "uuid", nullable: true),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: true),
                    ExternalUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    LastActivityAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastActivityUserId = table.Column<int>(type: "integer", nullable: true),
                    LastActivityType = table.Column<int>(type: "integer", nullable: false),
                    ArtistNameNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AlbumTitleNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SongTitleNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DescriptionNormalized = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Users_LastActivityUserId",
                        column: x => x.LastActivityUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Requests_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequestComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<int>(type: "integer", nullable: false),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: true),
                    Body = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestComments_RequestComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "RequestComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestComments_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestComments_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RequestParticipants",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IsCreator = table.Column<bool>(type: "boolean", nullable: false),
                    IsCommenter = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestParticipants", x => new { x.RequestId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RequestParticipants_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestUserStates",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LastSeenAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestUserStates", x => new { x.RequestId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RequestUserStates_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestUserStates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6d455bb8-7292-cba0-2fd0-c18e40ad8fc5"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("020e8374-59db-6d77-bdf8-b308e278b48c"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f63a6428-55d5-847b-3d09-3fa3b69b66ae"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("277e8907-d170-780d-816d-92111e007606"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4be2eea8-571d-6936-ecf6-5f99dd829c04"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5c08b275-6c25-972d-2aef-7e2f6ba227f2"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c4996dec-2489-820e-eb83-6ddbd1144557"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9a803c96-ca09-9208-d9e6-04083a5a11ea"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6b5c2528-7420-0e22-f136-6db9b89d9d7e"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("56a687bc-652d-9128-d7fd-52125c518a1c"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cf595b62-3932-5723-49f3-1eba81bbf147"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd8eb2e5-9d1d-95ad-93e3-4129f18ca952"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("286bf3c1-9d25-a8ce-d78d-964db9d15b37"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4f830df7-7942-6353-1d84-946f271c084e"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d2e7b90f-8c28-863f-f96f-14627ac06394"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1e80ad9a-a13e-b515-9262-1c0dd6e51bb9"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7d283a60-e2c1-e3f3-6b1f-3c988a89cfc9"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2277af16-56ba-327d-44d4-3f1e1dba4366"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ebc2634-b7d3-12c4-3487-606d1ed8d376"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a4f7e266-d355-e402-865f-da369963cc03"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f29aff69-bc10-d860-692e-275a4ffa4138"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4585dcb2-e48c-b99a-8995-91f56931e11e"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("02088d3e-a9d2-44a4-0975-41c1f695ebdb"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("262c50a8-e2a9-53d6-2bce-82d075d843ec"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e999453e-9193-fbfe-a533-ab541773943e"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5f2c94f9-dfb3-2e40-06b1-9dd70a9f9f62"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("443fb612-30f1-1b13-4903-ad55009dceac"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7beaf728-5c50-dabd-5ec2-f5a5138c0822"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("44b73f87-3a4a-c6d2-e3cf-b37ea7937563"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("582676cf-cf72-3c09-1055-5a3b2de29a6d"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b48052d3-aab1-dc24-9188-17617fc90575"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7464b039-de31-f876-5731-46ce62500117"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a4c47b7c-30c3-0603-cf8e-79863111f251"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5a954c6a-9afc-43eb-8f93-74047d725365"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95256bc3-92e8-a83e-e26d-b643d93d621a"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8f6dca18-fe45-9659-260b-41dd9a66cbf3"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e0a0ca63-aeb9-650e-99c4-d95a791c4a2e"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5025f51c-262d-e7c5-ad27-70bddf43b476"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("92cbee43-6e9f-a236-a271-f9cc5bb5d262"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f88fb399-23c1-ef86-3e56-93f63f8bb809"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("318f1b81-ec0f-a6c6-05e0-805f67b8caab"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3a06decd-3d51-f70b-c0ac-d640e8bd6f40"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5dbf9b93-4c1f-e317-37ed-97b3e641772c"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8425f968-cb8a-a4bc-3174-a0b07641102e"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6261b063-df52-a8b2-70f7-9619312364d2"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f9d91f6b-172c-e91f-6c90-5257aa9e3e01"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("08a6111e-0d45-a09c-86e6-979cd47183be"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9320ee39-2c29-9fb3-1269-cf38f6cf32d3"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0d392bc-7142-5407-4e11-a1f2c6d8eb55"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2ebd9e4b-a639-f66a-0574-69d765fa4a07"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bd081306-fb20-dbb6-c886-da6a42b080af"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("13bde2a9-4729-31d3-5fbf-6e0ab74437a0"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c5221bbc-e459-1944-cf36-b874dd93247c"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("30e02344-8dec-c2ea-d203-22a803f93b48"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("163cf2d8-cb34-8509-0df3-8b681a0ae74b"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("616cc758-2766-8f2f-71ae-2f99b98aba63"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b9afe726-36f8-0b50-3a3d-a6eeb53b8e37"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8ccfdf94-55f8-bd0e-cb7c-8052d6d2ca89"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9edd4162-4e67-68e5-67e6-65a023fa3d41"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cd93553f-b424-dd6d-00da-1fd3de10267c"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cffd7f2e-95f3-28a2-e315-699f413b13ff"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("50894ac8-809a-d90f-79ef-8169b16b0296"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ff4eed4-1cc5-d453-6ee5-947784437a60"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b233a0ac-9743-0b2b-1055-014c23f4147f"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cec2c46f-97dd-347a-53ea-c2b8a8ee6bf2"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("798d3376-ff64-b590-f204-c46bef35339a"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2fbfdf98-8a93-ded3-1eed-4582f6ec2dc6"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fb35de56-6659-1268-9f28-97e0be7d870c"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f5f8842b-1294-e4ab-95e1-2b60fa955b09"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1546df1d-4e92-2d14-9092-44d6daeb689e"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e11913ea-3d25-8024-c207-30837c59fee1"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0c683b52-4b31-ea62-1421-f895264e8b29"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7c9b3a2a-91ad-0f5a-cca2-d2a9ab7f4379"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4a089459-cc6b-d516-42c3-22ead8d2c7ac"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b63db7ba-321a-46a2-7e6a-8dc75313945f"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6c1087d4-e491-5a75-293d-c80ba2e59acb"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a9dddd78-8c93-9f48-fe2c-7d6cd303c32f"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dfc917eb-2be2-6a79-2f66-8fba157d5778"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("de923cf1-09d4-8a9d-14a2-d4dda9eb8556"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("26666288-7cc7-7af2-3404-8e026f1cb6a7"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8d90f3ba-2a9d-9f11-e8e9-684e2d1c013d"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d0716532-ca01-997a-75e1-45ca0b56e999"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("244b20d4-551f-dd7e-fd6c-81caefa013e7"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("84de96d4-42f4-1056-b509-d68d5ded3457"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("42a71bd4-6390-1880-cd7c-e5e19a4092b1"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("79457a59-de2d-667d-2813-a79cd70427cc"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e0cefa09-426a-e3dd-a65a-498708d55e72"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e2be036e-1bfa-44bb-c8ee-abb86ba87fbf"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("17e73900-e7f3-a01b-2710-cbc01e43f7c5"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f160bbd0-5316-bf0e-2d20-498426f48241"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ff6d2e5-dd61-c1de-c556-0a8f1169aa43"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("70f56e2f-1c9a-05dc-7da7-c6347e3f1947"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b257b1e3-3731-c980-137d-c4d0197753ce"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b9fe8d2e-01b4-ed09-7d3a-23cfdd6ba221"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d9b766a1-cf5f-a185-028b-8303ecb12b4a"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a6bc32c4-deb2-21c3-b5a9-0aa463d6247a"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5ef2d5be-debf-facc-6a06-0055acb63c74"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("67dc3cad-e46b-ad78-c9bc-25a65e487114"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fab2408d-06d8-5ba8-78ff-db4b8d0a5c58"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("219f3b33-dc1f-b3c2-143c-582a023e5b25"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c3f25109-36ca-e223-69a9-71a3d4083f00"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dcf2a737-2724-2310-abec-6d0204ff4bff"), NodaTime.Instant.FromUnixTimeTicks(0L) });

            migrationBuilder.CreateIndex(
                name: "IX_RequestComments_ApiKey",
                table: "RequestComments",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestComments_CreatedByUserId",
                table: "RequestComments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestComments_ParentCommentId",
                table: "RequestComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestComments_RequestId_CreatedAt_Id",
                table: "RequestComments",
                columns: new[] { "RequestId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestComments_RequestId_ParentCommentId_CreatedAt_Id",
                table: "RequestComments",
                columns: new[] { "RequestId", "ParentCommentId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestParticipants_UserId_RequestId",
                table: "RequestParticipants",
                columns: new[] { "UserId", "RequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ApiKey",
                table: "Requests",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CreatedAt_Id",
                table: "Requests",
                columns: new[] { "CreatedAt", "Id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CreatedByUserId_CreatedAt_Id",
                table: "Requests",
                columns: new[] { "CreatedByUserId", "CreatedAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_LastActivityAt_Id",
                table: "Requests",
                columns: new[] { "LastActivityAt", "Id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_LastActivityUserId",
                table: "Requests",
                column: "LastActivityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status_CreatedAt_Id",
                table: "Requests",
                columns: new[] { "Status", "CreatedAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status_CreatedByUserId_CreatedAt_Id",
                table: "Requests",
                columns: new[] { "Status", "CreatedByUserId", "CreatedAt", "Id" },
                descending: new[] { false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_TargetAlbumApiKey_CreatedAt_Id",
                table: "Requests",
                columns: new[] { "TargetAlbumApiKey", "CreatedAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_TargetArtistApiKey_CreatedAt_Id",
                table: "Requests",
                columns: new[] { "TargetArtistApiKey", "CreatedAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_UpdatedByUserId",
                table: "Requests",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestUserStates_UserId_LastSeenAt",
                table: "RequestUserStates",
                columns: new[] { "UserId", "LastSeenAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestComments");

            migrationBuilder.DropTable(
                name: "RequestParticipants");

            migrationBuilder.DropTable(
                name: "RequestUserStates");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ab86e6a3-8db7-45f8-96c5-4c8c24aad02c"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bbe1e77c-4926-4651-8f99-339076ce3071"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c3e8d097-f6cb-4433-a0c4-1027af3f4d48"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("01151aa8-e5ca-4080-abbc-d00de25dd71e"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8b43556f-f837-4eb5-8772-9195640d1ec9"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("812c68b7-dbbd-445a-9a6d-27c8b92652b0"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("87334367-dde9-436f-b70d-221c0f545f75"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8ccda668-846a-42b1-bb99-e6669333ba46"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2b666643-9c50-406f-b41e-982a28ce016b"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c2277087-7e24-4aa1-a38e-adfde1fe17d1"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b2dc208c-2faf-47ea-a53b-f678df13a69a"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("df2b5ff7-5e4f-495f-a888-e345263400c3"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7c4cbce4-9a51-4c9c-8a4a-8143d0caa2d9"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cb5c6bdc-e1bc-44c8-a8b3-b00952327731"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("04b42977-1df6-43ef-9e2d-14b1e7749f70"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("277f8792-2728-4ccf-b6ed-7ee248d9fe6e"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2912734e-fc72-4c90-9f4f-08266470416d"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c10673aa-3368-4a67-a2a1-8f96496ad316"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a7124ff8-2175-4af9-891c-236cb2e084a3"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cafdadc1-9a54-4e14-a888-e1194ce03216"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("92cb6779-ae09-4dfd-b592-0f34dfa113a6"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1656aa25-6832-47ba-a31a-ff20812ccba4"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("81b42fa4-115b-4852-879c-8f8b75934b9b"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3a80ce82-af97-4f90-84d7-cdcac41f1a0a"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21360186-a13d-414b-a577-b62c4ccbd196"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d7dfcefb-0ed3-466d-89a7-ede56a3a2161"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f1ae95f5-d448-49d9-a49d-a33eddf4c2a1"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ce8e5366-1b72-4b0c-9e10-f93ca5e2063c"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("32e932fb-34b0-4470-863b-ea16bfe62409"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e5ccbd15-f27a-4402-9aaf-f5f57523507c"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e4b61432-5f00-4706-8e9c-c93b4887b542"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3afc6313-c6fa-4163-9746-211c3ae74558"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("01132001-65fa-4a01-8fe8-69e829a21156"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ae3ac120-574c-45ae-8ac3-7d9c0fac23f9"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2f243faf-62f1-4566-8db4-fb17a4ab5eea"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bc2e6a24-55fb-4fa9-a1b4-5fc7f341213c"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("400d174b-d536-470b-88b6-f8caae0c51c2"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2e06445c-abab-46ca-8d50-bf681ecb373f"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95c26b5d-d4db-40d2-a003-232d3cfb1a39"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cc4bfc27-d7a8-4a87-98f5-dcdc644cf6fa"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6fee39e9-66bd-439f-b294-d4d6089a8776"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e5e7237f-25af-4eb8-95cb-36de42cbde13"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("51dea999-3be8-4ba0-81fb-d1d7952a4214"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a658cb29-fb20-45a9-b07a-f240c9417d5d"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("54f9d081-6f86-45a2-892b-3c83a82e977b"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("321f5c3f-824e-4424-818b-005e450b0cd1"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6fa93a6e-e959-4b2c-a7d8-e471beb9182a"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("98f810d2-af4b-42fb-9158-9371c52b50a7"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("be6e6b26-f789-4331-a92c-a2b77a6b7ae8"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9c24aff8-fbd1-4a0c-9538-35d56d8add13"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b56c049c-5e14-499f-9a61-034955f2f29e"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("94565548-5f7c-4496-932a-a3b74c16cdf0"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("916e4478-8124-4d62-8279-7e5441964080"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("39d5e825-67d8-4c7e-9f56-26f35511162c"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("10c11922-b1c5-4c62-aaa4-c22df6c1998d"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1d972398-17ce-4740-8536-70742bd48cec"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d9a5f081-e7e1-4fee-a437-a8e245c89b0c"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2727e201-55ed-4bd6-b821-c772ad05f8ad"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fce88900-b3b0-4daf-9604-3dfad8ae951a"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("99406981-0a94-4f45-8f18-9be0c5e2c0fa"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3a672b19-4d42-45a6-aa5c-6b0a37163a9b"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d617562a-bee9-4e43-b321-e0cfbf6eca7d"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aff2dbc4-f035-45d4-b507-d7d6538ac362"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a4af1fab-7485-4c70-97a6-d1162122aa25"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4ce3fbe0-28e6-475b-9517-3c30e2faabc5"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e9e2c056-6e5c-4087-b3b6-53e8ba8c22e1"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("53e50386-b221-4b2b-b263-2b42fb40d633"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e5fe6aad-8ea6-4521-8ebc-550a227bf0d7"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c98eb942-590f-45ad-9b25-01f44233c0fe"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c7581f24-4468-4136-b701-b370ae946373"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("23c0b18a-468d-4215-8ab9-bba4379a9242"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6adb2ff3-d348-47cb-8740-96bc6069ef73"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1dff0f47-1ec9-4836-9923-4b15fd26b9b2"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("91d3f41e-c87f-470c-9c31-f442029b83c2"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0090921-bfe2-4b82-b0ae-fba8b6a36d1e"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d54d265d-414a-4a8f-bcc0-708430163f08"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9632bf02-4689-4ac1-b7f3-a7ce4d0de022"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("031f214c-2fd0-46e0-959d-837eb1e0d3b7"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ae4359fd-ba3d-4fca-a9a3-9cc7f8e39887"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("633fe582-6ff8-4499-af23-6f45e9f38655"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1d419475-8dc1-41da-8a37-a56532327139"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("867d066d-6104-4672-a96e-e66b89ee448d"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1e5d2fb4-04a0-4bf8-9c2d-727ff2720c37"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6e7cefe5-1895-4425-a8b7-b011d5f16c13"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a2a79efb-6800-4980-862b-dcac276e7c4b"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("edcc74f7-e598-489f-b408-c7f68ba1d751"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("797f3f97-d85d-4728-98f1-41cdeb6065ed"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("714aabff-108e-4fd8-83e8-66c96d97542a"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("393e5946-293c-4168-b0aa-717ec47b8055"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fb34b9fd-1275-4f2c-83be-1bb939650207"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("337b7a70-279c-4459-b9a7-1c10dc916397"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a446f7f0-b32c-4180-b345-5696f1eb80b2"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("548b3311-c33c-4a73-b9d6-75497e0992e9"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("67c66af6-8c6b-4306-aa1a-f44a977a08d9"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3c68bd13-1447-44c2-80bd-7356ae30e644"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f0f838a6-8e06-435e-8177-a866c8f7a14b"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a7ed03fa-4916-4db7-8699-c79d3428f96c"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("de683364-6a70-4410-94f9-e3d8fcc779cd"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e5fd9d95-1fb3-421f-b060-16eb105f64df"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bbb229b3-02d2-4ba9-ad5d-3bac81b0630d"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1b23e0b-cb89-4923-a1e2-ccab9bd925eb"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("76000339-6630-4c2b-97a8-b8c9061d80e0"), NodaTime.Instant.FromUnixTimeTicks(17667042347640456L) });
        }
    }
}
