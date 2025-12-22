using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddNowPlayingFieldsToUserSongPlayHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNowPlaying",
                table: "UserSongPlayHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Instant>(
                name: "LastHeartbeatAt",
                table: "UserSongPlayHistories",
                type: "timestamp with time zone",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNowPlaying",
                table: "UserSongPlayHistories");

            migrationBuilder.DropColumn(
                name: "LastHeartbeatAt",
                table: "UserSongPlayHistories");

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
        }
    }
}
