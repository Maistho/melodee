using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddChartUpdateJobCronSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102);

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

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { 1405, new Guid("5163b103-1991-47b1-b095-736b2ec7bb2b"), 14, "Cron expression to run the chart update job which links chart items to albums, set empty to disable. Default of '0 0 2 * * ?' will run every day at 02:00. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17666713099655591L), null, false, "jobs.chartUpdate.cronExpression", null, null, 0, null, "0 0 2 * * ?" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1405);

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

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { 102, new Guid("b63297b0-af57-439d-bbad-64bfebd78194"), 1, "OpenSubsonic server actual version. [Ex: 1.2.3 (beta)]", NodaTime.Instant.FromUnixTimeTicks(17665249660317879L), null, false, "openSubsonicServer.openSubsonicServer.version", null, null, 0, null, "1.0.1" });
        }
    }
}
