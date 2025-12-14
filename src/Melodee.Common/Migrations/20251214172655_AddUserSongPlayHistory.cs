using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSongPlayHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSongPlayHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    PlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    Client = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ByUserAgent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SecondsPlayed = table.Column<int>(type: "integer", nullable: true),
                    Source = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSongPlayHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSongPlayHistories_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSongPlayHistories_Users_UserId",
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
                values: new object[] { new Guid("e43e1f96-d164-430d-8729-9cb1f94ad26d"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2e1e79e1-7530-45bc-8a26-ac71ae4c62f0"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2afc297c-5f17-4c69-91b4-261c6b08485f"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af93096e-9798-4467-a8bb-53b915a57542"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7ea06c4a-6110-4d35-bb37-b348f568d15b"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3e5b73c5-dc94-4c2f-8937-a17b0217f394"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a177ab19-c88d-4c38-a525-6e24d7556249"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("171c5d1f-8cd7-4898-8b3b-dd155fcbf53c"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("88d5a545-7a27-40cf-be0f-cb81b4ab16f5"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e2ee2b12-5925-4360-8f54-f2c66f703a07"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fc7a6f03-cb7d-442a-b496-10ea9ea85b45"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5ba05533-9e9c-4e7a-a187-2642f5438b9e"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e553eb2b-bef9-4fca-9b52-53c4d57a3edc"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b40e5c83-7cb1-4f8b-8c46-4fcf7fc7539a"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e7b71773-bb0d-4add-b5f9-dbdc1455688e"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3f1b8d89-d99b-472e-b5ba-50883cbf7272"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3f118f05-fd57-4d26-b231-e043e124c230"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8f776d5a-69f0-4fee-8814-f0e39e69e2b0"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6cf43baf-b724-4de7-b8b9-60617fcf8906"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a421a1f-22bc-4481-8712-18b569893469"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("27ec9b29-107f-4a76-9f4f-97cebfb5aa00"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aa6bb8c9-d6e3-42d4-9efb-cbb16c2a755a"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("53bb8439-66d1-432b-bc9e-3e43b37a5066"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("db41019e-89d8-4656-bde7-4ebc93fbb490"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e0062797-1222-494a-95b3-619b8666fea7"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7a4f8aed-330b-4367-8582-03a3d0a63d36"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ebb26a3d-1306-4fab-9da9-6ced3181f074"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("59220e3a-aeaf-495f-a05e-1867d9bfbee1"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7329f19b-b8d5-4a01-941f-4d9cde4f5949"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a73cc166-6d15-4fcf-b7e3-832d8c0c5b33"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a76498c4-d21f-4447-b37d-4863736f1fe3"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("673d057f-b766-4e17-8b10-9e759555dc58"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ce605a2f-062f-4b94-8533-3cf506b25d20"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("071cb421-b259-427c-9e0f-1f71f4180813"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ee6fb689-952f-45cc-aec3-900375125e90"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1904f25b-fe4c-4d3b-b697-77e791682cfc"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0ad12ae-7b7f-4a2d-b288-20dba05bfc38"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("50aa22eb-b428-431d-990b-6e8028752b25"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7ede8008-fa33-42fe-ade8-8d9c8ae86443"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f7501577-6eb6-4043-8610-6f48e9f3bd5f"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4537847b-3841-4c4c-8213-6cbc703bb7a0"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8009b84d-4145-483b-b1cd-fa7715dc33c3"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("761e5d2a-40db-4806-8983-19d38cb6b9cf"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7b4982eb-626e-4266-b19d-d48df7677a50"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a5f999c1-4be8-4d51-9e7d-0d8955939116"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6f78a321-4587-49bf-a1fc-fcf799ec89e5"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b7ff065a-763d-4cb7-85da-372c082e59b5"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ff71bfa5-a58a-4b81-99a4-4ec553685301"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b025525c-0d03-4ba2-a390-017837cadb63"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f0949d3d-9f32-4aa6-acf7-d9a5a08150e3"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e587d3be-38a4-42fc-845f-52cd5a9b41ea"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("495304a2-41bf-48c0-a564-77515144b4d7"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("33d6a011-69cf-4f17-947e-79c2b394ef81"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5feea9ab-e025-4aa4-b96e-3195379f3bd5"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d873a075-7f01-411b-9004-32ffabb03084"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b69ae13-a59e-4855-82be-39b9b2069385"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4c46d322-3eea-4b9d-99fc-09090ff2d111"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a0da1ffb-9a91-4d67-a14f-b6203f71b9fe"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d978a43-f74a-4368-a4e5-f3edbca9ea9f"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("587cb39b-b707-42df-949f-d58b77d07ded"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("06fe43bf-9116-4113-b55d-e2ff634384a2"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7d67277b-57e9-4058-b334-90a26d4ccc20"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("23b7a3ca-4ffa-4040-a96d-154c94909d5e"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c4bfff23-9925-40d2-b85b-4553025585af"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7b9992bc-aa05-454b-b441-aacfd8e4b5cf"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7fed1fb6-956a-44de-bc00-262bbe1dbfd6"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b9acba9f-712d-4813-8b03-b13a8ebe3d65"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("313583a6-12db-474a-96d1-7588aaf59bfa"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5671eab4-fceb-4bba-a810-858f914fba06"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0c59982-0596-4ea8-9cc6-c55fe2fc4c13"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("78c56522-f242-4f53-9140-c20c3b0b4aa5"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25e56255-f33e-4852-8bd5-8bf725f92613"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("414b40fa-7f9a-4a7c-9b60-2d2366443209"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("36aef1e7-d582-49ac-9695-bc52bbabf1f6"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("765d6352-e047-46ab-9c6f-1b2174f4ab0e"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("961b982b-bc70-45dc-8e3e-bd02af2b1c3f"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a4d7865d-4e3d-4bc1-9a59-6df55671a5b2"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ab3badeb-4fe7-4423-9bdf-7d1bcb859f2a"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ce72612-2d07-4ad3-abf3-de21104338d6"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b2ffc4fa-9d50-422b-a013-1e9e5c40c775"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("896580d5-8f5f-4752-8061-a1972791aa25"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4d30291e-6e30-4d85-b4f5-2a44e83938b7"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("234fc299-a213-4d86-9fb3-4b9111d552ba"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("859ad9eb-4e24-47e8-8aac-4b3a4b7a1353"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("adc1b117-14e7-40e3-89da-55e722a859b5"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("36080c24-5efa-4b7b-81f8-69ce051d0898"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4a2a226c-5ab4-47bb-b312-f6a7e0eef11f"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("33540ef5-4c6f-45ff-a609-b88176444939"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6d7befc5-1ddc-46cd-9179-473e878f0214"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a3688434-ff86-4e7f-adcd-2726a8b6eae1"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1fcd6b0e-57e1-4e12-ba0d-c893673bbb24"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b962205c-cb6f-4e70-a71a-32e739c634ce"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1b681217-291b-42b8-939e-b8114fbbd4c9"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("768dc3f8-1be8-4a2e-a108-0d64ef59a6df"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1c30aba4-7ac8-4a7a-9fc2-03699daf2dbf"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4566e3f5-9750-49d0-8a38-1678386595ae"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1a9bc93f-07ea-4094-a8f4-ad9014d591e6"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dfc139de-a210-4586-9fd7-7f071925a457"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d8187958-cb42-4bd2-8956-d9b013920eef"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ec1f3235-d19c-4741-859d-c8e6de9a2ee1"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cd167563-cca5-44c6-9ec0-950754a5b5bd"), NodaTime.Instant.FromUnixTimeTicks(17657332146305413L) });

            migrationBuilder.CreateIndex(
                name: "IX_UserSongPlayHistories_PlayedAt",
                table: "UserSongPlayHistories",
                column: "PlayedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserSongPlayHistories_SongId_PlayedAt",
                table: "UserSongPlayHistories",
                columns: new[] { "SongId", "PlayedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSongPlayHistories_UserId_PlayedAt",
                table: "UserSongPlayHistories",
                columns: new[] { "UserId", "PlayedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSongPlayHistories");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("329a2843-242f-4f81-88bb-88137a708a53"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("60f98131-92e6-48ed-a5f6-d1983268963d"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a7c9c03-ab88-4507-84fb-76578554916a"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("54e396a1-9ae3-48d7-aebe-f7a12b337c92"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4660f54a-62f7-4465-8532-29bb2d348f72"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f85f422a-f228-411d-afc5-9c7d81f6fafa"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5b212eaf-6c07-4f75-b105-5a0cb90a6bda"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a9617f71-6be8-46d1-959e-65c61c16649d"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("73621bc3-4888-4a4a-93f4-4c5f483488f8"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b623c050-d2a1-43c4-ba81-6bb622955742"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fef3e9df-50e2-4738-b3b6-13e994a708af"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25f34c8d-00d0-421a-b328-0b601a97102f"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("18b39fb5-737b-484b-9422-583f9704abc7"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3782dfe9-a413-4aaa-8c62-aafa12a9e736"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2be325ed-1ab9-46ca-8798-8775fe7b7a96"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ee02f26b-ff12-4efb-a2b2-63fb252681ec"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8dda43cb-9c3a-4d23-b682-de228bfd6b76"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6f1ad238-fcd7-48fe-a753-9c10ea0562ea"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("873c9889-62eb-4c9f-8542-5f3c1df06a60"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("118442ac-2477-4632-a8de-3d86209c31d0"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d4e9c6c6-cd76-45ab-8ff9-b05ef9912e55"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ae0ce27b-16a8-4ffb-9827-b48cf7e5d90f"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("810c1d9d-8f27-47f0-bebf-42b9b317198a"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a6c88e2-2e9f-4d53-94db-452d983d4e3f"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cc37d749-7112-4d8c-9654-ed1d2f24d89d"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("06099fae-ae67-4fb3-912c-6451de68bac2"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("05dc2a67-7769-43b9-a85e-f6225b757ed7"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("093bd246-f772-4cbb-a603-d885ccfb3f6b"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4505af9e-4331-4b80-acca-c989feabee99"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1db14117-8ae5-4940-b221-feda084a122a"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2cb27bf9-1ff2-482e-8414-2b5a4886e770"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("13e61c6f-9b58-4610-9055-3c5730708585"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("578bc175-9963-4e2f-bbd2-863667dc1810"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0c1822ca-9cde-45ef-8996-31c20c932303"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a8630463-5d75-4428-81b9-230537e81df9"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("04e051fe-bc77-43d6-8380-85b075a6da89"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ef8c62a-c244-4636-8ed0-2ed71bb97a69"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c21019fb-4430-4557-817f-13f73879ad64"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a0607bff-ba19-4aa3-9368-3173480df7b0"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6d47c052-761e-451c-bc11-77213e7e0a16"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0be5d81d-891b-4ef0-b827-a1bd9bc31f71"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ef7ff465-51b1-41de-983c-1d74c59b0135"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bd2d30e9-8655-4fc7-8bf7-1332de1fcba1"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8393eb50-536f-468d-8096-fac84e0168e1"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2ea454ba-abc5-4607-8e13-ae8c11429eca"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9674b63c-d566-4fcb-b007-8bd2e7e27519"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("68b4fe9c-1a4f-4734-88e5-b567bf7f44ea"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e2d6c01e-1ec3-461a-bb6c-e2ec1ab335c8"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c6bf8e43-9e92-442b-aac3-c751b455cac2"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("18861bb0-1720-4183-97c1-7e2523840d88"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("718e93c6-6725-40f3-a5ab-bc64d0935b6c"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3bc68c67-b3dc-4f1a-ac4c-0b72d5045e5f"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("feb4de04-7c64-4d9e-8baa-eb6af704b388"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("137b9a5c-cb2c-49ae-bebe-417a299fed98"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("203accf0-7bb3-444f-ae46-5de15449d827"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ec7b224a-e24a-483b-8fdb-3b17c4d8e58c"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a12e8339-63f3-47b9-9516-54ec9b84db34"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21d0a56a-0265-4f2e-b346-d18f5232cb6b"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7fe70b28-46d5-464c-9494-efbbdb9467d1"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4a7da15c-3f2f-4adb-9fa2-a7b123c5d752"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b891a388-d57c-449f-883c-1f6b608ee644"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5dcfefde-a584-44fa-a0d3-d6108649553b"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7bfa20a8-0629-44e4-ada5-db620be80ab4"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5d60f8a2-044c-4bbc-891b-8d1c527981ba"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5e5476e2-ba87-4eae-bfea-0488a990e665"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6ec8de3d-3e87-4830-9569-5c51d49a8f37"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bca90fcd-802e-4752-8528-fe7deded36e2"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7f38dd40-388b-4a45-bd13-5e04894048bd"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d3d832f-da36-4b63-a8f1-b53316c69a35"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0ab75812-a15d-4122-928b-38cd2c6337d6"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9e36991b-150c-4ab4-9130-e44281450659"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3b111339-270e-4d02-b5a9-bf12e1aa18c0"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1b7fe3f1-6172-48f0-8b4d-1c80c99bdf3f"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d18ff30d-1a92-46ef-b775-1628ce4cb99e"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("03198d98-9d45-45cc-b19b-2d60cf98099a"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0892f9b0-9c4d-4e3b-a2a7-0e977c9b705e"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aa98d256-fba4-42c8-bb0f-18bb9962bd58"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ea345dda-b6d5-4880-8f39-9789c3e0a65f"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6cbe6296-fbde-492e-bd11-b3a51b832a25"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("889f8342-10d5-43de-aa69-484bbeb1795a"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("33d078b1-b6bf-4d45-b3c6-ea131aa084b7"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f5e80d4a-6328-4c17-8ca5-6e6564255149"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3f95e4c1-47ef-4c31-aea4-8d6116cd4fb6"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("02555f50-7059-4d99-9aa2-43ee15d2e833"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bcdf407d-3f58-4014-a79b-19238da836b2"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4b655453-132d-46b6-8b35-62d80a45158a"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("098ece85-1b94-49c2-a7be-b9b8a756e40b"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("61f39acf-7b49-4272-88b0-8be0beb89798"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d5e8b1f8-6fdf-4b99-9dd5-7238bdab08d7"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2ba49c76-abec-4ee7-a4dc-6d05dbc23391"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("80894815-6a60-4a0c-87f8-9e1cbbf973e9"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("89cfea23-966f-4bc3-a050-65e2556a342b"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("214790ae-256d-4a53-a1fd-fb1df6a52a65"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4208dc43-8377-4365-a53a-e4648173c798"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("26bcdb98-7387-46ac-adf6-ea98a00a5033"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0e6af31e-a65d-4b37-b913-19c0f551fffc"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("59d5b859-9673-41ec-b91c-1d9d9db64572"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c249bf62-301a-4651-bebb-2900deb1dfbc"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("949cd514-59dd-47d4-991f-dd0458e16ec4"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aabdf76e-6fab-49f5-a8f1-8cbc8f96f7af"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ff1a4174-4261-4b56-8211-39da92246785"), NodaTime.Instant.FromUnixTimeTicks(17657306259226320L) });
        }
    }
}
