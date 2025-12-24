using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddCuratedAlbumCharts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Charts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SourceName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    IsGeneratedPlaylistEnabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Charts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChartId = table.Column<int>(type: "integer", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    ArtistName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AlbumTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: true),
                    LinkedArtistId = table.Column<int>(type: "integer", nullable: true),
                    LinkedAlbumId = table.Column<int>(type: "integer", nullable: true),
                    LinkStatus = table.Column<short>(type: "smallint", nullable: false),
                    LinkConfidence = table.Column<decimal>(type: "numeric", nullable: true),
                    LinkNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChartItems_Albums_LinkedAlbumId",
                        column: x => x.LinkedAlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChartItems_Artists_LinkedArtistId",
                        column: x => x.LinkedArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChartItems_Charts_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Charts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2b7fa178-88a1-4e65-bfdb-5754cd135d11"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("78908ed2-d6cc-49fa-b92f-b78284b47313"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("de0462ec-cf30-42f5-8900-5fd8e5dc7efb"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4385ba5f-44c9-4336-bbb1-007f1a53814f"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("47a51d2e-68c1-4de3-b85e-c2ee0f9991d1"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b15ac59e-c889-43e7-830f-f6c8c168e684"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e1a92361-4db6-414f-bcb7-33e2af1c4f48"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6e0d8c6c-330a-4740-a7ce-20e38bed57f8"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("528d1c45-68c1-4d0b-ac15-602f52af971b"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a14fdf87-e73e-4d54-9023-845747d382a4"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a5efbb07-c164-42ec-9506-19d4d6e341dd"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("33adfa3e-0bf1-421e-80c6-1e0181e58a0d"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("38bf9a78-4a00-40ee-9501-bad0e7074f83"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6e080d77-5e92-460e-878f-bd6f04846448"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("49303275-2575-47f9-b1a2-fd2ff829152e"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("395cde49-0cd1-4b90-a042-28553efb5336"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dd6b584c-19c9-4ab4-9674-132479ce48ae"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("70e6832c-437c-4a8c-8187-e9d7ddf0b527"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bce057e0-b51e-4a55-85c3-f79533ded572"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7326069f-c498-45e0-9445-fabc9a44593f"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("98dd968b-34a7-41f1-b43b-71841467f22a"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e1c9f4aa-1012-4c94-8822-f82d4dccb255"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4497a402-a98c-4498-b24f-dd781d4b46b2"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4cdf36cf-68d1-4f58-bcb0-d673fa94491a"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a57679e0-fa3d-4d7e-8970-430bb376965b"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("61d0a9ee-3693-4a1d-91b1-b503f9c44a17"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1df0faf4-a6b3-4416-8b6c-27c585635c70"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b428b348-078a-4fcf-9f11-73fd0770c5cb"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("90d1343b-d385-4765-a76f-cf4b91f323b7"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b138b9af-75d1-4318-9185-ccd4274188ec"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4459e9d4-ed75-495f-a8da-f90a587cedb3"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("df7d2ed7-402e-4f77-9864-f3a8ae8c52f6"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("168df76d-04f7-4aec-8dc1-ed8e7c97c768"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("05413b1a-44eb-4f32-a7c0-7a1514f424af"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b63297b0-af57-439d-bbad-64bfebd78194"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("53839be7-a219-44e2-bcd8-a04ecc1b902e"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("09d2eeb7-a8a6-46e5-b06c-673b48b07cde"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("73646ce7-04be-4781-aae0-9e202ffe14dd"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9b49c08a-2bb3-4e94-ad83-15f71a873c53"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a039716a-e011-4748-82ac-04febf161f8f"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5f9de0aa-8803-4269-9d01-c8d2f557b932"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b27d614-24a2-4e85-95a9-7c4dbf671023"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("03ab9596-21ec-4967-9874-fa3b14bab58d"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("38d993fd-9ad9-4415-a8b7-3412f55a04af"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("99e6e655-79c4-49db-ba9a-3eb3c99a0926"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2ab46df1-4cd0-43f8-a8fb-35d15fb48555"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0e5d424f-9dce-4e90-8f0e-db607b96b4b3"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("76f7aec2-b4ce-4939-a92a-fbecf6526817"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("620a737f-fbf9-48ae-8973-612394549b24"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cfe457cb-6454-4b8a-a7ab-599c0ddc2ca3"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ab4e545-3428-4b7d-a99a-75abbfd98758"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3b41288f-ee44-4061-b637-8c8f6d2cbcf4"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e8a0c703-3bbc-47bc-bc96-22e04bb6e0ec"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("81afb29f-1677-4e14-91e1-fdc3e2badfc4"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c5798e57-124f-4147-b703-e2bae9032c48"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("160b553d-a1b9-4503-b3aa-358f3ee79dcd"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f4ec54de-59bb-4f9a-a3d5-fe38f0264410"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8557730a-5e67-4020-8f42-874ff36056fe"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3bdec0f1-8831-4d84-8921-efa46c0bb758"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9917564f-1f5f-49ad-a70d-ba3caddf79db"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ae5e6033-cb43-4d1c-809b-e4237911fade"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8ba2a697-4fd0-4ea1-8837-e800864239fd"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("840e3ead-48c0-45e6-8a19-5f9521c2ddcd"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("54f7b8ff-d41f-45fa-817a-b0caf7fdb6dc"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e242aaca-e390-4f05-9e20-f43c288c7f52"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e00af932-2990-4637-a8a7-a26a6cc87b1e"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("02b8d12c-11d6-4c53-bb8f-cd331b857a82"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0df0a907-aba8-401d-bf98-ca50226db55e"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ded55c08-34d4-486a-9c15-b7242ded53bd"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0cb3bdd9-d1e5-409f-bf7f-f325101142b2"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8c36f0bc-14d4-4afb-86e0-ce1bb18ea421"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("487cfd4f-3bb1-4031-bf28-c2e6bb014f22"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("80f180d6-e5b9-43b8-adf4-39ac92c1174f"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("63e293fd-eaf4-481e-b644-82da75b08bc2"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d3aa43af-a9da-458e-bfd3-a36ebd7fc00a"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fec36b40-24f8-49e8-9b99-a5ef51680b19"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("98ad56a7-ad64-4e0a-a4bf-8420c90796b9"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("849187a1-9edc-45b2-84d0-ea57049cef9d"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7095c602-09c3-4aaa-b509-37b35c2b77b6"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7ceb5084-e617-4b7e-be0c-1aaa879609e9"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2c2bdfb4-3b64-4d25-919d-622dcde06477"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e65156e6-d343-4115-a6b8-06b2734e0650"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8691d936-e229-42b8-9201-a584b7c94466"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("858f63b8-3cb3-42e0-a3bd-889d6208997e"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("75e5428d-d24d-4431-8f72-554ee073f059"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ece7a17-a24e-4c4b-8db9-f4302004bc6f"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("17234fbb-90c5-4242-9a66-b18d303f9473"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("342473ec-c4bc-4d1e-ab2b-e95be227c949"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fda3b090-cf44-435e-b359-4cd0e8789bdf"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("76aa79c1-51ab-46fc-afff-f2a7b30d92fa"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("947a596d-2f39-46ad-b3e4-6d3400bd730d"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("66510176-7167-4d11-ac7d-e2b63d4f9aaa"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fab284d3-0b66-4f6a-b903-423eef9ef8ec"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("152c3b8e-e947-452c-886e-f7927fbf0e20"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("76895d12-efdf-4a65-a460-966a13a1c0b1"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("019844ce-a892-49e2-9376-6500d8eec81c"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("939ba694-a466-436c-804b-c075b798ac48"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c9f8fe05-99f3-4c1d-a584-3e34bcedabc4"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8756c3c3-4bc5-4c84-835b-935eead473fa"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("343866c9-23f1-4a0b-82ca-f69c212331b7"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d310bd1e-de6a-4f2f-8e63-7cfdc4933fc7"), NodaTime.Instant.FromUnixTimeTicks(17665249660317879L) });

            migrationBuilder.CreateIndex(
                name: "IX_ChartItems_ChartId_LinkedAlbumId",
                table: "ChartItems",
                columns: new[] { "ChartId", "LinkedAlbumId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChartItems_ChartId_Rank",
                table: "ChartItems",
                columns: new[] { "ChartId", "Rank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartItems_LinkedAlbumId",
                table: "ChartItems",
                column: "LinkedAlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_ChartItems_LinkedArtistId",
                table: "ChartItems",
                column: "LinkedArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Charts_ApiKey",
                table: "Charts",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Charts_Slug",
                table: "Charts",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartItems");

            migrationBuilder.DropTable(
                name: "Charts");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d31c858a-1040-42a5-ad14-a36a223b707b"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b7fd5b9c-ffa8-4769-bc4f-a31aecefb505"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e810dbb3-2733-45f2-9908-2952e00b0561"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c6704ce5-e107-422d-a5dc-6148d68371b9"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2729c92d-69a8-4239-9e02-a5f5ada605d3"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("acda228e-1017-433b-8b38-d85769a2a86e"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("312e56e6-7820-4a71-b855-a78569eff78d"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b87c894f-58c1-4811-b95b-4b7d3df387c4"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c1cdb37e-14f7-4326-9ce2-d720ac2f98d7"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c2720ffa-62f4-4cd5-b2cc-808893a7d302"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a08fe1f2-c7f1-4f7b-91bf-65e25ef9f1ba"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2a9b51db-5edf-41a2-9fd8-d9edcfe66379"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dc083fbf-193c-44e1-9fb8-0c51acfeefaf"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("94e62ecb-204d-4ee0-a991-215c804e5230"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b0389b4-5d22-4aa4-880f-c36e69198d2f"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ec9da394-ba5d-4dff-938c-2325b0e0574c"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("67b58d60-5e98-40bd-a534-92c4dcb1d887"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b9494afe-9efe-44b9-882a-687eed20faa2"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("14072eb6-f551-49c4-95d4-baa96fbc9eae"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("42a1895f-4e0a-40d6-b073-aca33e6796c4"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("69adb5e6-4a03-41c1-b47c-4d22c0344165"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b0785b8-ad99-4422-b0de-44810a0feedc"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c3e4996a-5ec9-4450-affe-d60c47f8c224"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a50d5aac-f5cd-4c29-84ef-9b41df290454"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95567a26-ca33-4bab-9fc8-060b5e5b421a"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a8028984-1ced-4a0d-8b7f-b23990fe0f31"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c193974b-c247-468f-85ed-7f5328f4fd1d"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd09f21f-3278-4662-a6f2-f449b7ccbb3f"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a9b17918-ef46-486c-ac79-fff5e485dbd3"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ede7c8eb-a58e-4de1-b8d5-105a7d7c96f6"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ada1655d-f112-42d2-b71d-6fad792412dc"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9f89cf61-8353-4615-9345-aa96ed1f493e"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0d758ed-d1c8-438d-8d28-41ae3dc1fd96"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("abdbaf83-b933-4977-95b8-59ad20896279"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6ddc0b5a-54de-429c-a14d-59c432b89b3c"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("82e9e173-e66b-4b0c-9f50-90d49d72de8d"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("db3d885a-924a-4bee-8697-697240f0a0a7"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ccebd060-1caa-4a7f-a623-0679cda56154"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1b59187c-e009-4e4b-b40b-6d5278c0c812"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("39d593b0-aada-427b-9b1e-3f873ed5a45f"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3c93d298-ad85-40cd-8012-897e3218be07"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("63e45340-51c0-4892-a439-146788bfcee1"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bd61b556-2235-4a8d-b626-d0c0faf2583f"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7c1bc8c6-79e9-4011-9257-11c45c40b5fb"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e44d3d63-dccd-4d36-addb-d56d06c5605c"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9e7fcbd7-134b-42c2-a9a6-7ba7cac22c1c"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8bfa1d8b-10fb-4364-96cc-4c9e53b6554d"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e5e8cd09-dab3-4460-8cc0-2886727c8c9b"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5af84a57-0aff-41b4-a3ae-938d18a1e355"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1f34283f-057e-4386-b994-030d99db95f2"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("69b439ff-f9dc-463e-a6c5-e6e0b706d68b"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("faecd7aa-db75-4c0a-b115-d311ffba8bad"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b324885e-603e-449c-8357-67e46873d9a2"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8340f0eb-9925-46e8-b288-24e1533b7101"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25351593-5084-4e18-b29f-0f655e35a6b0"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("20b6e6b5-5da2-4a9c-ad59-0a48e6afb750"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6ea9f249-6353-41d7-a00c-74a55e4830cb"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dd663df8-fa0c-4b84-9b5e-1deb315e0d43"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9eab0fbc-581f-45af-9555-fee2179848b2"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("169ee347-ba9e-4efa-8e45-847ffa6cd5ee"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2a4bcb44-ca77-4ff2-ab8c-f295e984e038"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("09a84ed4-122b-4b39-9a59-337d0f6ecc7e"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3162363f-175b-44fe-9675-bce45a7a7c06"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bf5046d9-7d60-4692-b8c7-d9e9747751b0"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("56899be8-6a5d-4448-92d7-6db965ea3920"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("37c95643-50e5-44fd-8bd8-01ba116baf6d"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("78f38c37-9525-4af2-82ff-1f279425b08d"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("04398ec4-49ed-4a43-b262-9305e1a99998"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("53667144-74f8-42f8-8168-cbc5ff4e4c4b"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("92e2fc05-9a0e-4aea-a168-4ff05eb4f25c"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7dd9ba27-ee2e-4653-a999-e079a4ab6587"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("74549fae-4f0f-47dd-a871-6dc09a1a8f77"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("802f53ca-0edc-4114-8d24-f54c77ffbc58"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f11d7bbc-9962-41d2-959f-15cccfb745a9"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cd5cb5d6-6306-4815-9923-f8a95f7d5949"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bd96c787-eeb3-49cc-bcd8-ecbd3acb9ddf"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("66ce7be6-1c17-440e-bb49-498aacf11669"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ff6ced31-85a7-4050-98c6-c36e62a6f1e5"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d6b1caee-a419-485e-ae24-f1eaaa81b3f2"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("713870ee-0643-471e-a74f-cf7a23f05027"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cf8e5da1-6d13-473c-b94b-6867b3789bbf"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("83ad932f-5682-4e11-ba02-d3d1b446eb2d"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f6dda19e-9cb4-4f2a-b553-023db174c84f"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("07a46f93-e14a-4381-b346-c2b60fe0eb08"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dc9eda16-58dd-439a-a581-7e3f85764335"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("956e95fd-7279-4217-983a-ecccea11c4e9"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7c93d311-9925-4da1-88da-ce013767901b"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("32236fcf-83b4-4e4c-b3d3-c39dab13a1b2"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d63ec08a-f8f3-4cd5-b061-38ee82f15dd4"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b60635cb-d0bb-4f6c-8295-c4fa720046c1"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca7b6557-1373-4430-9a1b-3b6fd05b08b5"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("be22ac84-7223-4703-8871-3169974111d8"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6438272d-aec3-4c68-83c2-fc8360276964"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d5ecd96f-29d6-489c-ac16-57c04b62ce9d"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("49494de5-99be-4f6a-96d1-f4a27aa5bb4d"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e9dc0bfd-a629-4ef7-99d0-32a89f6893aa"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d776df74-9b6c-4a95-b6d9-6e56ddb978cc"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("68b2f917-d160-4625-a8b6-45c9c4ac4532"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("849161d8-3bb0-4ca2-b596-59be74a73fe6"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6a218846-bba0-48c8-829b-a7bda014e850"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("16d47559-4c6a-47cc-9cf3-9b061a120d5a"), NodaTime.Instant.FromUnixTimeTicks(17663498909437807L) });
        }
    }
}
