using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPlaybackSettingsAndEqualizerPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserEqualizerPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BandsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_UserEqualizerPresets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserEqualizerPresets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPlaybackSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CrossfadeDuration = table.Column<double>(type: "double precision", nullable: false),
                    GaplessPlayback = table.Column<bool>(type: "boolean", nullable: false),
                    VolumeNormalization = table.Column<bool>(type: "boolean", nullable: false),
                    ReplayGain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AudioQuality = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EqualizerPreset = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LastUsedDevice = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_UserPlaybackSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPlaybackSettings_Users_UserId",
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
                values: new object[] { new Guid("0e5fafc0-6379-4531-9b14-5aaff47055b0"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a5be89c7-cf1d-4451-b607-7893b976c3ee"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4c7a928e-9384-44af-98d3-81e0e277822e"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("29642bbf-8d8f-4d00-a3bb-5fa252f56680"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d531cb3d-af6e-4a87-a96e-320a36371336"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2aff0c14-11bc-4c13-9fe1-99dd888de81b"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d2332eac-9dbf-4e27-b361-f6ce3ee0bf08"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6684a15c-63b3-496c-af80-554632085e0f"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6ec600fb-629b-4eb1-a171-cc87236fded3"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("be000480-fcb6-4307-a622-2eb99c91227d"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("acdcf344-9de9-4d5d-9c8c-b47988ad85b7"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0725d3d9-d657-4ffb-b43b-ad7e0f7aa822"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd43bae8-04d1-41ad-b7b7-b6390c10f272"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d307119e-d5d2-4991-956c-e899c0a868ab"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c5d1938a-ac98-4f6d-974e-578fa7e86b9c"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6080d960-25eb-4469-b3a2-607e681cb48b"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b2f6b1ff-cac6-463f-a4d8-334cb17635e3"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ea9e2f32-2138-4836-a067-ec46c4c100d6"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c8320e95-ebb5-4aed-8d13-0d3dc31746c5"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2dd51106-3302-4091-8a5f-db0cd527fa99"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("604ec997-4d33-4ea4-aaa6-1724e4c5f45a"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("916cbff4-1e36-4141-a1c7-0f3ea0745428"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bf6d9708-387c-41de-a2f8-17f3bf4f9f71"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("85b0fe0b-2c72-4269-8bfd-3a13340bd9e8"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d768e5be-8a26-46ba-9edf-ecff0cfcd2ed"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ddc508c9-7883-4eb9-80af-824aaa1cac8e"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b7c0c395-f45c-4740-889e-80ecdc8c1892"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("56b0fa70-772f-4280-ba35-17462a7761ff"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("06da3533-2459-4547-9b36-1a6c55307cbf"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21123452-b9b6-4718-9972-ae1e1070b520"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fe7794e1-03f5-4e4d-a9ef-9989e0fa000e"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("69e65ee3-7124-4244-818a-a826e0c9a994"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("53306371-bd66-4964-893a-9b10d8124997"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8f13f538-783e-4233-9e5b-1683c6657484"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dcd813cc-7684-4a28-9f38-e3fca891d567"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ebe77efc-e379-4c67-a9cb-26bdf9ddcd78"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9673f581-b1e9-4cd4-9d01-ca3aa4a0ebee"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("508275e6-254a-4eae-9d2d-0d1f63b64303"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a3416e0f-e7fe-45dc-9ca6-b31b6f64bd8a"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4bacf2cd-12fd-4f96-8ee9-db7fba343b31"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cfc3dfb4-e698-4ba5-b12a-266be3eecca1"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fa0e8f86-c403-45fe-bdd2-526f725c0793"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6cb483dd-b3f4-469b-8035-28b72be329b6"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c62f51c9-6f17-42e6-bd10-1bd93593e233"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d5743b45-a247-4f83-93a6-7db88c214b17"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("287807d8-1bdb-41d4-8435-210625156dfe"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8f0a3b66-8f13-47b5-a924-b4c6df12a937"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("08111b40-cf22-4072-bb7d-472d569e2a04"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9e0a64f1-35e5-4ad5-9138-1474f7f422e5"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a72f9352-69ce-4d5c-81c4-2c539384562a"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4bb9a6c7-b8fe-46ea-8f2b-ef4ca5a29177"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c339e813-2971-48e7-8123-d258d927ad5f"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d44738a7-30c0-4b9e-ba24-0a8cc7252e69"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("49d21f0b-bc3e-4fcf-bbdd-8124920ccf44"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a96f9614-423b-441e-8904-8debfc06cd3f"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6ee0fb26-f6e4-4a11-a72a-977273e01093"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f4e1d602-b49e-4093-8386-f0b2fd409d92"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a2311842-b54e-46ba-9ad8-13a180c6af8b"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8ddbaaaa-718b-4c38-9c5e-a8929f8623f4"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2a74eab0-62a8-4328-9b27-d804d64082c5"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e136442c-d8fb-4f85-bd15-bcf53ea7032c"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("559fb90f-536c-4345-9e79-731969c05cae"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c7ce2689-aa48-4016-98fb-c5dc17d400e7"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d36e5b1d-cfbd-41f1-b5ab-1b7b593a581b"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c1937a53-4aba-4496-8d27-0de204813e03"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("022ad5e6-7088-4596-9bc1-f04b04589b19"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4ee42872-5e14-42ad-9784-5624cbc97705"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d86ca6eb-d39c-4179-993c-ef4d26f565e2"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("60c34e6c-c969-48d6-9ac5-623878348474"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a5559073-6d6a-4320-b075-8875aae152d7"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b1874a29-c370-4ef1-9fec-1729706a36e2"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("82bd4510-ef8a-41d6-8795-b138ba925a53"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f62d0061-5496-4032-bb00-6b2ff69044a8"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3dc2a534-61d5-4a41-81d8-7e41c6d3e346"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ea41f656-8bac-421c-9e42-9764a8195ef1"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4f6469b6-1dc8-4e7d-bde1-00185f66b299"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("18e309c4-14f3-4561-9221-9427f2f89f54"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8eaac2ca-1ca8-481f-9776-42dfe4617270"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9bb529ad-b17a-46c8-ada8-c525071f58b7"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3a6ad1fb-af71-4652-b2a0-cf32da6400b9"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("292d86c9-0559-4936-97c4-901c4bd571c9"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8d40ecf7-dd52-4764-98ad-4bebd425db48"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e24c81d7-3641-48af-856b-73e68b92f7ca"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0c17b34d-5229-4d0a-87d9-2dd1aa693761"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("01b000fc-9565-47ab-b057-429d07d6db2f"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8c0856aa-a088-48cf-a12a-79803d59cb8c"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4707c407-be46-4eba-8066-74008eca2e4c"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("01dd92b2-58b4-43b9-bcae-2c9fd13ca1f5"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ba6d3605-02f0-4ddd-8921-2f6c6456a2eb"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7fd4fa5b-7234-4252-addd-6ed906ca193f"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8788b455-fc65-4e0a-b549-2fb238250076"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("441affae-85a9-4c72-99f6-5ea5e6bbe489"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6756f116-c5f8-4992-9589-ee479d9956e0"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e9e8a325-abec-4247-ba98-e3d8e82f056c"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ea586ab3-668e-4b19-bf3a-32a7efaa0266"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("da382174-e63e-4a45-bb28-380fb0d1c364"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2a4a532e-154a-4b3f-a300-b4176afbf938"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("51d23237-dae0-4a1e-aadb-52385e6fb432"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5a1ca326-d1dc-4ac7-8a0e-25b37ada8757"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ed2a3a2d-d925-4516-b914-eeb72455df8f"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0203b6d-434d-4bf2-97e4-b62cb490c84a"), NodaTime.Instant.FromUnixTimeTicks(17661730176061653L) });

            migrationBuilder.CreateIndex(
                name: "IX_UserEqualizerPresets_ApiKey",
                table: "UserEqualizerPresets",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserEqualizerPresets_UserId_Name",
                table: "UserEqualizerPresets",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPlaybackSettings_ApiKey",
                table: "UserPlaybackSettings",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPlaybackSettings_UserId",
                table: "UserPlaybackSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserEqualizerPresets");

            migrationBuilder.DropTable(
                name: "UserPlaybackSettings");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("24c6151e-2ce7-4c31-ab8f-29fad1654cc2"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b613c924-e1df-493b-9606-db040bf99f36"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a6706d2-d64f-43dc-9129-24382129ab71"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("713cd3c4-6023-4653-beda-567207a78eb2"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2f44b873-0b1f-4560-bd0a-2d3949468e77"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("344bc2af-185c-4dfe-9796-9716db7e3eb2"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c1f6caa4-33b1-4dc8-8382-9cf930c1e202"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("139b96e2-27a1-4217-a5d1-0896e86227df"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b3d4b181-72a6-4f91-96a8-1ba2bc56da82"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("67a4fd6f-3977-4dcd-b273-c5ffa9398356"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0302c8d8-f3ee-4185-b8a0-4768b8828ac7"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("be4c0b73-ad62-4c51-97fd-176a1878b228"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f9644f02-2672-43f6-999f-a935612a8a0b"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f3498248-f390-4e11-a530-046ba606f89b"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5a113778-0e7e-4a93-ae93-080b225b86f3"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("da7e2a7b-4421-40af-baa5-d31b865c2266"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c43759ce-7c87-44d6-831d-7fe00f81d885"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f4aa5789-71c6-4265-bfd0-b3a3e105bf81"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e44e71ab-cd07-41af-9906-476ff31cd0e3"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b5f5d07-808c-4f6a-8ae1-b0802c3e7f8b"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3d6f5f75-d523-4e18-8b2a-6cbd4380f8c2"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2d68193b-2152-46bb-ba24-4f142d638535"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("43452569-f784-4b60-ac7f-c88b514b1685"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("22599817-ff19-4b03-aeff-56bad538aaf1"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7d549efb-643a-4e8a-9b0c-db90d8b67362"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b23c5115-9a0b-4dfa-8900-ca808c3a3a02"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("63b0ec1f-8e05-4fb8-abed-17530a761f03"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0d5f3958-bb93-41bc-970c-5ba30c14a790"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a288b007-b4f9-4a29-a5e6-e829f62a100c"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c9d0f882-0936-45bc-8535-e51ac8b72fac"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d18208ae-1d14-4917-990d-c3dd3c1e43a1"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("da028730-ef4c-456a-8398-4fb434c0f1ac"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1cc95ea5-e1c7-423a-8167-d2f118806845"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cf131a2a-0a31-4d24-b791-12521e3392ec"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5d3de41a-ac14-4b07-947b-103cfe52c1b2"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("13bfa834-6979-4714-8d03-964fe5f28f2b"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8dd406bd-1a07-4933-a69a-a53bf442997a"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6cb2b357-f979-4f62-8340-123dc5780f2a"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1a4e97f5-a5fb-4cd2-842d-d3d7e8952b2e"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1b84b33d-11be-4864-9b5e-8f640d81def9"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3e88afe0-d321-41fc-98c0-519fbb588a9d"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("81d3ebd8-3b66-4be5-b3a4-accef6cf4f47"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("97cb3b72-8037-45b9-9092-577d329a0875"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5d2911dd-7226-4014-9db8-127fc7a206bb"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b16aca70-b385-4fc2-b01e-f919d5f1c06d"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3337a84e-556b-440e-a528-d537927a8906"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd5084ed-05b9-44d9-b9d5-78ece190d961"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c7ea825b-f588-47c4-a57b-1ccdfd5ae62a"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f0d4d5e3-0e38-4f16-8afb-92d9cb45e272"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b88c3a47-de90-4b85-a20e-7a8e9a68524e"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2e8221c3-de1a-445a-9272-0014cd36f1bf"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f0d3a3a5-1396-4773-a3ed-9808f4d900af"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("007edc1d-0e35-446a-a786-0e07f8bfb1b0"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c2c51ccd-9530-49e3-a0d6-0bca43556fb7"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("605a5129-c76b-43b4-85bb-d2e44b3379d6"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("beeecd3b-98d1-4352-913b-075e20b92a78"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("05403165-d20b-46ff-b388-b25c52cd64ee"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0d160fe-2c85-40ab-922d-e7245ec84cf7"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3b152d53-3bea-49e5-badc-33cf9d619387"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5762e56b-9abc-4b34-bdc0-4f4096f56c08"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("37245896-55f2-42ef-9043-6a75844c505d"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ea39cd7-3dbd-4abd-891f-0fbd7e5f2531"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("124886e9-ad66-4ce7-aad1-a8a38b95db5a"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a807a6fa-da0b-4517-b793-4e8c22b9b65d"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0d535b03-5e10-4646-9391-560b9b487c3a"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0c52fa92-5a09-452e-80e5-85ab4c4b3fa0"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0cdc2c57-38b2-439a-bfde-234780b792b9"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("93c68dd2-c704-401b-834c-58099b166053"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b70f9d19-1cd2-4b76-8c0e-0ec10fbcea0e"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ba965db3-3eb8-43cf-aca9-5fe90b9b9bf3"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0e803620-121b-4182-82b7-40b2f56f2c0c"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e7a82920-3d5f-4d53-bf95-f200920985e2"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("da2d49f2-1cf6-4146-832d-6115a1ed94e5"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e7955aa4-3892-4a93-9d34-4aefeb069db7"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e82f1b9b-c4dd-43a1-974b-f5aa11ddfd5b"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bc9c9be7-3ee8-43bf-9c99-69802863a6c6"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8cba2c6e-f9f0-459b-b4d4-f9568f67e47b"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bc92ac4a-394c-41f5-8cde-314a7e33d6aa"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2b4b0bd9-3e41-4603-897d-6a47e9b71953"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5b1d08ac-fa3e-4534-a251-3a9413e84f61"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("85488604-9599-4c5c-bba7-91f650b607b4"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9336952d-7440-432a-aa61-8c971d23a6c0"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4d57d67f-20e4-48c1-94dc-dae065b0f653"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("94a5dc6f-1b39-4f7e-9ca4-ac63f8ce490e"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a4820570-01d4-421c-b4e5-3da47f89c4e6"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("15f6448e-5d79-4d72-aabc-c2a2193fea4c"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("eba79d4b-8f85-4bae-842b-dc9aae7ae535"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bb5fdd88-7271-4b60-91bc-559341501081"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("57b5bb26-3d28-45bc-b939-d9294fbf3848"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c57703bb-b92f-48e1-99e6-11de3e6f2fd9"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d2f21447-1c40-4ae9-9908-6a2f1cb352b9"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d9fd2bae-5fa8-4e2f-a3e8-6771fa0b62e7"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("22060311-f2a6-47fa-9a66-bb42935660f7"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("247eacca-359b-4a68-bc08-0f242336a22b"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("abe6463c-d1bc-4c56-b878-e66d8cc160d5"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5c8e51af-8775-4f27-80be-b973e878db23"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b83129a5-d7f9-4fbf-88a0-c207fca05989"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("daadaf13-820d-4d29-bc44-205a0c37138e"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d315428b-879f-444e-b484-8bb82f27d557"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5826f3f2-895f-4203-923a-93d409c525fb"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1e48fc83-95c8-45b7-97fa-2d633c0d9d5f"), NodaTime.Instant.FromUnixTimeTicks(17661621751534507L) });
        }
    }
}
