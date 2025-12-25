using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddStagingAutoMoveJobCronSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("75606f0b-454d-4664-ad34-18c0c14425d7"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7215a5b8-62b8-43d6-975e-6dbba50984c9"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("be013b41-6145-436f-be39-830ce26045fd"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f55b11ee-736c-4403-b312-a44d3a92c5a5"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("05e1d601-4405-4f92-a362-3f09dcaf5bef"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("16f5be68-2a5e-4afe-b7bf-3b1d95d1ce72"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2a351e37-a133-47d3-9ef6-dbe836b0a1ed"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9a5f7bf4-bbc2-4a7e-afe2-f44f2e27229b"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("46b37649-0307-41bb-a209-f3a8d3c9e318"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5bd07296-97fd-4a2a-9ebd-360bc0b855d5"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1daea6c3-940f-4ba7-89c1-2b1588c2b251"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d5a573aa-9d5c-4e2f-9a88-a1bc8e24f2ef"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("587c480f-1c6e-44ae-8cf3-3410a4a1522c"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("66a062d2-ef95-4f1a-b80e-c4afab01a448"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f75cc694-4446-4724-a64b-8db69bfd9708"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1c39453c-31fa-4480-891b-518aa4d7b941"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("272425a5-ae69-478d-b35d-8f32219578fd"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e2b7398f-66c9-4445-afe8-229dad19d11b"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0217068-d7ee-4411-9545-05a2acdae328"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1885590a-e75f-45f3-a018-cf580c40e68a"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e350450c-566d-42e3-9bec-2d3cd1646394"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ec82f91-7786-41ec-8899-52d4c89c0ed3"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("524cf007-6f32-4ca8-93b7-833c3853bd25"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ce671dc9-1f2f-4493-8395-aff05507c679"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3988cccd-3604-4962-86e5-d2d3d9684aef"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("18d38624-349f-4d92-aae3-a3481f0b5ea3"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fca4e31b-d8e3-49cd-9b97-cd7b4a3c43b0"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c33080e0-1766-46ec-a9d0-b3e767f5f9af"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f7289a42-b383-4b9b-8414-63830991937e"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("75540d3e-72ea-4d97-a988-4bebee559f27"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e73fe4a8-cabb-405e-903f-52ea54486eb2"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f917d8de-2e88-4a9e-b121-b5ad9522e581"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("341a1e91-7bc5-4d1b-987c-c5925aae6616"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a5e9a356-38b0-47a9-8fde-eedb9ae35a4a"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("16702226-b74b-48a8-80a5-8af813ca5993"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8a8a5be8-950d-4921-ae29-7126fe3121bd"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e359af84-c73f-488d-a9de-367c6794e3dd"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("14165e9f-cddb-48ca-805f-f8f5528f9758"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4d8f7945-202c-4878-a6fe-fd6f670c3e58"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("513aed22-97fd-4472-89cc-8d9639745a03"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("31238932-1ace-481a-96ce-7d925aff84fd"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("30f850bb-3f38-49e5-99c9-6a712595b0d8"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e07ef6f3-5746-493d-9786-e5f1a2e1b6d3"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ff6f6fc5-af28-42f6-9697-4fbb042d1f84"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c3cde2ee-000a-40ef-902f-e9a4a8189faf"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d0a7ee4b-af96-4ba1-87d7-15006899f7f1"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0bb16945-6606-4494-acf2-d6d08f5aaae7"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8e55936e-b8c7-45ca-adc9-7acb340675f5"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2d4942fa-726d-445d-9907-440bc1ae8294"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("77cefc18-03d3-46cd-8d81-aa6114ca5899"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("48ba53d9-9872-48dc-b7cd-017d8e91b8c3"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b96bff27-2c38-48fd-abec-e09bb4dc99d7"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5052e2e0-def0-4d52-a1d2-72adbec1a34e"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aad8a26f-4429-499c-9a09-ee2706574a34"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e8cedec1-03b4-4754-bfb7-6ea48ffed40f"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af36d794-a461-4a8d-bfde-d55d59593ec8"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("167a0ad2-b1e6-48e8-b880-ae19d3889b3a"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("50bf5ea7-ff5f-455f-b0bf-8b89aa6d6f2a"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1fb34599-28d0-4b1f-b45e-0f6a9cd02353"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ed9fe7f-2c4f-41e8-a3fa-41c6fea18ae5"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d87ffdc7-9b24-49e0-a17e-13182bda8c1e"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a9b85f68-4dfb-4d2f-a192-8dd4310521de"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1de8ae15-a97e-40d2-9814-b950f87a6a4b"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("09c0b6e0-3769-4b45-ae56-ef03a52782bf"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b25fcca5-ed1f-4093-ab82-b27026408c1d"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f611cc30-e494-40c2-b30a-f238609f9fa2"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("74d31471-d76b-466e-9ffe-47bec6447d25"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4269c30e-4e61-48ae-9436-92cc5bdd6d21"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e6b94d98-9067-4393-a9b3-947d20f62ece"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c14be5e7-2201-4fa6-a600-87ef0a1d873b"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21ddd29a-ac2c-499a-970c-dc92e869ee25"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4891ba74-28a2-46fb-ac41-053eeadc080e"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("39ba5626-48d5-454d-99c5-67c3930afcd1"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("23452dc2-2ff0-4788-8b8b-468cae61282d"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f0a5c060-ff75-4e64-8c2f-316554dd3391"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1b9a73ca-821a-46b0-934f-82f319d2100f"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a8d4e65a-1cfb-4443-a6a9-a24dbde4f792"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("075814a0-6398-4d49-a1de-2d38403b06af"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("142b53b4-a404-44ee-87f8-db39073668b3"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5e7afe64-76a9-4924-9260-48a955ed4c67"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c9a75755-0539-42cd-83e5-c119fa556f0e"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b577e779-6794-4e27-999d-4f26d8aceca3"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("64789b76-77fd-443d-aa1e-1bbbd745a22f"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5f4862f8-e736-495a-9c32-3939ce2b82e0"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("889a3709-648e-418e-a6c2-ded42c646e9b"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e64d13bf-bc17-4d37-83df-7478bde38ac6"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("df1b9c0b-7bc4-468f-882c-a68cee60ca78"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fc18dc6f-602a-4c9e-b7b4-5c8c8457a47f"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2e63e398-794d-47d6-8cff-e10004b29352"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("81b2a096-f84b-4d77-a0f5-b2ec79a1dfe1"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f1e009d9-3aa7-40ad-a35a-bfd1ffae9e75"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("878a4e0b-7237-47d3-b2cb-ac854a488054"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d976436d-0234-4e5f-9a49-82c490ffd3c2"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b02d5751-a03d-46b4-b0ab-ebe5e5ad27b4"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("964008c4-f688-4b4f-8247-da90204dab26"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6571f377-53cb-4caf-8d88-287b6d18ab7f"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1c685e85-ea07-431d-a73b-7c68f35a94bc"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("610fbdeb-1605-4441-9297-a3a95c628018"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2274fb4b-ca84-4e14-9df0-a158583b10c8"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("580a8c14-a5d9-4c03-b2c6-642309f9c2ca"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("da3586af-c07a-4312-abb2-c1984b1c88d8"), NodaTime.Instant.FromUnixTimeTicks(17666731196080941L) });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { 1406, new Guid("8ac2b01f-d88b-4a48-9a60-75dbf213ad94"), 14, "Cron expression for staging auto-move job. Moves 'Ok' albums to storage. Default '0 */15 * * * ?' runs every 15 min. Also triggered after inbound processing.", NodaTime.Instant.FromUnixTimeTicks(17666731196080941L), null, false, "jobs.stagingAutoMove.cronExpression", null, null, 0, null, "0 */15 * * * ?" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1406);

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ea8ddc26-7f8a-447b-907b-02e06f4abfdf"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9c008de7-e0b7-4d3f-ba93-211b0b25d09b"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("65c03a82-1e42-4511-a94a-ea20161a440c"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2874781c-e86f-446e-a388-254a02fa5ec6"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("48cc1cba-9f4e-4084-b4b1-f6e741479d3b"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dcd86164-c769-41c0-ad85-0facf28b3776"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("206383a4-b7cd-4a57-b2ca-8b3605a83fd2"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("446c4241-edfb-48f7-90cb-499bd353476a"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ce1991d7-9e38-4579-ac6f-05e1f5cd070e"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bd94209a-7d8b-42ad-8b2c-c50e5ad684e1"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c11974fc-3ff0-4ef6-aeac-867471990860"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("806cf3be-3af0-44b5-8c5a-913423848ae3"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("84c6d6b3-ad6d-49e4-9077-290521e45413"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d7122268-4905-44be-a926-1294cc0236c4"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d02e36a7-bae4-47cc-965a-c6e807b58237"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("10243f59-ff55-4212-bfda-80afdabe7c1c"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f033afe0-1998-4cdb-af4b-1cc52b31e2c3"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9e3bb2af-0a01-42bd-98d5-8768eec27495"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7a5dcb8b-723f-428d-83af-967c589f9746"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("823b5e9f-2c20-449c-aef7-d147fe6833b0"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c5f37936-01ad-450d-b8a1-16286e141a38"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7a90e7b2-e753-4a43-9693-fad01a4845e0"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fff43cb8-8678-480a-a815-97bde98fa5c7"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a71b840b-39cd-4deb-a1c2-b3ca5ff79789"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2bf87238-6e08-420d-baa6-3b12481d6df7"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cdcdfcef-ca16-4d63-95f1-13b5676697cc"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5e7d3ddd-2a53-4344-b14b-2b5b315c2251"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d3c252b5-d1de-4a0c-8192-d51d94682130"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("91a906da-26c2-4699-b1b6-c4017ff640ef"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5f93ed77-a1ef-4add-82f0-68df9acae413"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7d2ffb03-c143-4b9e-a2f7-566c7ab8aa4b"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d9611522-9891-4fde-bc00-7236dfc21d85"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("869593a1-fae8-4745-90ca-7b789616f141"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aba5dd2b-b745-483e-aa70-6833d78a6b7d"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("73870487-6ffb-441a-92e4-c89647379644"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2f929b7f-b999-4e17-b6ca-fc34fdc8735d"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e14695a8-3112-4868-b999-c0180500ebe2"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3550e541-c01c-4613-800e-74f638336550"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6e576384-2375-46ff-9ccc-090d8671f927"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8268ea4e-a1d1-4e17-b5ad-2dcf94dd6015"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("934dc980-50b2-40be-bfe1-4688041c77a9"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("219a3886-c6be-4b6a-a44b-fa52d5e49bda"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6b9ab4c2-2b21-449b-a2b0-1b9fcc9ca631"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("63f9c8ae-0565-4b9d-b85b-f0b296271fa6"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2c554b98-5c9e-427b-89c0-240c9f6a4ab7"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("75127e2a-4103-4f78-9d92-913ee55f5e64"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2cb5dae6-da3d-4118-8680-38d28ec21ede"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f2bc9161-203f-4568-88bb-f3726041ee51"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ab9bf23e-d7fc-47b5-a8e7-19231926069e"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e185f32d-40c2-4161-ae05-827d6a6d36f3"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("42f63ac6-0090-4b6c-b8e9-fd10f4493637"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("559dbcba-5aad-4c56-a165-081ad0116f81"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3849d615-b2b8-4236-bf58-541d0ff142c3"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8201f855-5c29-40ff-9b51-6bd15832c24b"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b29576ad-89e9-460a-a4a1-5b8ea5fdb9e2"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cd4a9fdc-e73b-44b9-bd77-41031cfcbe19"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("14ad16de-68c2-4ae6-9787-ed1214b384fd"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e9eac02f-2431-46d8-91ba-af26d25f6549"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ac13fa87-1a5c-41a2-9b76-f26992c64fb5"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e3f92539-dba6-4122-be16-a895de5ab81b"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("db081197-857d-42cf-94ad-6058c1cf9210"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fc41201c-186e-4a37-91ec-041f61b779d1"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af9f0d31-87bc-4d39-b67b-d89b801388c6"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("19cecbc0-f902-4e03-b4ad-2c5ea4330e63"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("04a034f5-6fc8-4ded-92e9-4f5063018de6"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cd3e8665-75be-4b87-8c96-8650ac28caad"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cda8d2fc-2bac-4abd-80db-fcb7fc2859fb"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("14279f3c-ee10-4b15-8ada-0d45773a46e8"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d2327955-ab45-41fa-b598-fdab67ddaf06"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f503724d-272e-42d6-b267-cfbbebbb7d81"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9fff5c56-7f2b-4679-929a-d51d7b3abe12"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("23936f81-1aaf-41e8-88de-0ef009653451"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2248146b-61ab-4d07-9a61-9f29aa56bd9b"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d888114d-f83a-46a5-bbe3-bdcae40f56d3"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2ba7f8c2-7560-4f59-993f-fa443c97a92e"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b57609b5-ab39-4f7c-8958-c0255bf092f9"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7333af95-eed5-45bf-b5f8-d234df3b2c88"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("803d8df3-2a01-4599-92b2-f1ab5c6c8880"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("de49d2f0-4685-45d6-8b90-415fda1e91c5"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3c5e8b1a-3faa-464d-905c-54d8f545ca10"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("90c30519-a9a0-4d46-a39d-1b159a7d9989"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("661470d1-c91a-45de-84fb-470045040f46"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9caf3831-8d42-4ee4-8f04-67119a358e35"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("00deebf2-f2a2-44a7-88e7-e9eb61990bd0"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5e31afc5-edea-40e8-b2db-7b52bd15ab85"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("047cf6db-e41c-4b20-a8ff-431c2f954c39"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("892c7201-4c7a-4ad0-b255-9a64d7659076"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6adcb941-8e55-4de2-8c5f-c1e321e42393"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b6074cb1-547a-4e35-9068-13257dc4becf"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4cf5f6cc-d518-4a1e-a32a-27658ce8ce7f"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("86038e94-ea70-40aa-a86b-2c936662cea8"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e70f359d-b0e2-49ec-85ff-7156ddde5d57"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8166013f-5d5f-4994-bf17-ad7dcc2693f4"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b0fd4d72-bef7-4adf-994e-0726f958a804"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("27ef6eec-a92f-4906-b667-17df63c41992"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5be221da-e63a-4ec7-b4f2-b284a1b4a8da"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3528501b-ab64-479d-a917-cb9f4bb22f55"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("28920dc3-820e-4915-bf29-04e790f3f9c8"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("67aae7c7-0002-4c12-a625-7a688457b448"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c9acfa21-aae5-4332-a4b9-10053e7d6109"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5163b103-1991-47b1-b095-736b2ec7bb2b"), NodaTime.Instant.FromUnixTimeTicks(17666713099655591L) });
        }
    }
}
