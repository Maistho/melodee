using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetTokenToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "PasswordResetTokenExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StarredGenres",
                table: "Users",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StarredGenres",
                table: "Users");

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
        }
    }
}
