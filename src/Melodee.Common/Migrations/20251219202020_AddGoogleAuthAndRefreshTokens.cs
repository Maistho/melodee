using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAuthAndRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    HashedToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TokenFamily = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IssuedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ReplacedByToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SessionStartedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSocialLogins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    HostedDomain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LastLoginAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_UserSocialLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSocialLogins_Users_UserId",
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
                values: new object[] { new Guid("52d35d45-48f5-49b4-97e8-fc36f1b738a1"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c19b3a83-3b6f-4cf7-9888-b5a64fd408b2"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("06a0b286-8390-43f4-83e3-ffd8b7960106"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a5139761-83cc-45af-9459-de9058e800b5"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("85bc06c2-6fa3-4633-8b60-eebc253b052f"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d302744a-637f-4fca-9035-c8066011e770"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3d26e81e-21ba-4e3a-b591-e487b5de1b61"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4d0d6574-1b24-4391-98d6-a87d0033b54a"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("787efe87-459f-4606-8aa5-5955a12da478"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d127c240-bc31-45a3-aa2b-3d533f5f2c78"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0d8ad397-d3f9-4847-b529-b7ffede853cc"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("931e678e-cfb8-4d7d-8ed1-239008c8e149"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2f349409-1bea-4ede-8272-fac5605a81ec"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6645e413-b95a-4b37-86dc-2f1298b3743f"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("56151614-e237-4327-8364-e2320933fb83"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("68a57d3e-95f7-41e6-b057-0d9644c3d073"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7a272fcf-7b46-4c0c-9f4c-1f7e19b6f7d3"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aa93040f-1b10-452c-9734-0c1121212ea5"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("34367e1a-894d-4903-98ab-55383da7fe09"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("819bd1e0-a4f6-4371-8ee2-203df250a6c6"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("32c184a3-2282-4c08-997c-110ce783ae0b"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3fe90d66-7ac4-4720-b944-7d39ee983303"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("34eac89e-4ceb-4dae-976d-00c31f4cc75b"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("66e702dd-693d-4ebb-a530-0345ab6eccfe"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f9fab75c-e663-4a06-93cd-ef01c15d91b3"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("770ba547-f5c7-467f-a766-f592971963e8"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("98fdd199-f32a-4927-8920-9bd20ced0a39"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5d36d9dc-e52b-4a2e-a1a6-098da3585130"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3d5a80fa-85fc-4242-87fb-17bc03f4c9c5"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("359c7178-9629-403a-b602-17f8ee71d2fc"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25a6749b-a577-4abd-aba5-61529aa37217"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9dcec42c-1c4c-4d92-87db-a3ca90702cda"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2598d098-23ba-439a-bda4-e5f8ba61ac80"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("866d18f2-51a7-4cbf-a5b8-368439ba3d56"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a026619-9466-4f77-b19e-ed37d517f8bd"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ca650c4-a138-4c2e-9f68-eb2fe302e9a9"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3f169ef9-c03e-46a3-a3df-dc72d7b7f294"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("866fe0ba-61cc-4059-864c-3155f8a0aa87"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("06c1e5da-1f53-4fa7-9754-e5a52d51446a"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6ca65c22-98e2-4cf2-a506-a866947e00f3"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("86560aa8-f2ec-4953-a6a7-22adcb41dd97"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("de4ba1b2-c63f-49e9-9e1c-0d96ce0c5013"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("59f36531-ec84-4a6f-88a0-05271309d1f7"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2e674dcc-2a83-4ab6-8f10-12fc1d4e469e"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d2b2a06-b86c-449b-a7a0-c1d55f3ec91f"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("46bef548-a3a2-4e21-ae93-09a45cbb1734"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("265ee3ef-2db3-4454-bdb0-609eb5b3ca3c"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2775a578-eb8e-4e8e-9016-e8a66688d8cf"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8f281603-bf6b-461a-95e8-92720fd43402"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9c96465c-1a22-4515-82b5-9349dc16274f"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("216c7812-0062-4b4a-8752-67d56c0212f4"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c163ede7-8cd2-4ef9-b0a6-c62c5f5d3517"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c93dad6f-d2df-4119-86eb-0ccd313247e2"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7d4809b6-df13-4837-ba96-4ea04565ac36"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3a38ab27-1729-490f-8427-121982fb5592"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7022b9a9-119d-4753-8e22-ccff58393278"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af456f66-b703-48f1-90ae-c0e6ed13ef5b"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("878e494e-90f9-4378-94a9-195b77523ad9"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ebb3420-53d5-4945-b9cc-b60a7fd93183"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e0304a3a-a65f-4660-ba31-c0c9ccbea254"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6b686c2e-5e9b-433f-b67e-d50eb5ef1fb9"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f0c79460-754c-4b73-9972-c67b96dc787c"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("98373fe9-71d5-49cd-a904-35946ea36028"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("59dcfb20-33fa-4660-bd59-865b8e3ea59c"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6def98f8-91c1-4b92-b668-0e00d803e3a7"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b61d425a-02c7-4c7a-a1ce-7fe74cfa12d1"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("166358cf-98b3-4599-991b-949ec79cc086"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c3762cb5-e7c5-4e3c-a6de-9d5466f483f5"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd8bd01d-8563-4d62-8605-9d961c7bc9b6"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b7989fab-89cf-4bdb-a369-cea6778b354c"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8e5e1b0e-604c-4aef-8e61-f74c72e65773"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c4d27ae5-e2b1-43d7-a89a-152464121886"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("459e2e2b-0950-40ca-ba5f-7445a6cb649d"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ca815ae-6698-43ad-a8d3-8982d47c07c4"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("49837505-c529-486d-a53e-5a94950359e7"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("accbb684-6612-4d35-a5bb-c462f0626010"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("adf4e26f-5d0f-41fd-b0bf-ca83e506ca8b"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("62f095b9-58b0-40ca-9dd9-22fccb0ef170"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("39245895-1281-4b37-93b6-2d9ab567a5bc"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4366519a-899e-486e-8da4-01d5398f7823"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dbff4182-d9ff-42f1-a5f4-d0f2e6fcb9d7"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("06f58319-2089-417a-b09c-909a46024efa"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ce324e45-5e59-44d5-aedd-81fa49635db4"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("14daf968-4f1b-4733-8c79-ba8b0b1f7df9"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3b94a3f7-080d-4edf-9f30-5798fc476e12"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0db1fb15-d52b-4b0e-b8ae-81e38699f305"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("49c84a51-d784-42ed-9f89-c7b6b4086cf8"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2526f9fb-4b84-4a89-ad5f-8471983ba771"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("74ee5080-68e5-4cfd-8938-c9bd084a839b"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("45f415de-9df5-4165-951f-ae15a563a80d"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a55ddc04-24ee-4eba-af69-91179c8b560c"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("352f3d40-8211-45f9-bc30-a802845119e3"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("66407cf9-6e47-41c8-bcca-a1f80207fb2c"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c35c828c-396f-494b-a794-6d271d49f465"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8a77c0d4-32f3-4499-8aa5-cba974b86b17"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9bc634aa-6a6c-4d33-815b-71fd44986a9a"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ddb97c94-a789-4151-814c-9bc0aff5dcd6"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8beeee09-a1e0-40ce-a775-1480ee1327d0"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2cb0ac68-f60e-4c9f-98a7-5093a199aeb8"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5fc94445-0829-49c1-87c4-447d32e5ca31"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b7f331a0-e3c7-4f1d-a767-4888a290799c"), NodaTime.Instant.FromUnixTimeTicks(17661756191074044L) });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ApiKey",
                table: "RefreshTokens",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_HashedToken",
                table: "RefreshTokens",
                column: "HashedToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenFamily",
                table: "RefreshTokens",
                column: "TokenFamily");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSocialLogins_ApiKey",
                table: "UserSocialLogins",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSocialLogins_Provider_Subject",
                table: "UserSocialLogins",
                columns: new[] { "Provider", "Subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSocialLogins_UserId",
                table: "UserSocialLogins",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "UserSocialLogins");

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
        }
    }
}
