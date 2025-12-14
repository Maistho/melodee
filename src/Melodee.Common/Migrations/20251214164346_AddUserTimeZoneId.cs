using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTimeZoneId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "UTC");

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

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 919, new Guid("889f8342-10d5-43de-aa69-484bbeb1795a"), 9, "Is Metal API search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17657306259226320L), null, false, "searchEngine.metalApi.enabled", null, null, 0, null, "false" },
                    { 1102, new Guid("098ece85-1b94-49c2-a7be-b9b8a756e40b"), 11, "Maximum upload size in bytes for UI uploads.", NodaTime.Instant.FromUnixTimeTicks(17657306259226320L), null, false, "system.maxUploadSize", null, null, 0, null, "5242880" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 919);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1102);

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c812ffa0-1cec-4284-aa1e-71a46527758f"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a566bec-f5bf-40a6-afae-c0ae342e9c52"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1dd63cd8-1f62-441b-bfde-58f89559ac69"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca1b5183-2816-45a1-9c6f-e00c1727dcee"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8464bc2f-47eb-49c8-8e8d-39ff88c84a5a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("75db42e1-f0fd-4295-a7db-6104656dd57c"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("71c2bd5b-3069-4244-a313-790f739646b7"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9287aaa6-0982-4fdf-8f68-7dc64c514abb"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("80ced0bd-8ee1-417e-bbb7-a55b1699fb05"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a0547b74-6be9-48ca-8a33-dcb91361c25a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("28feb17d-05bb-4d77-9298-2de9b6d91846"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dc679c03-d4dd-4ea2-88eb-9ad2590ba414"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c79a6018-2b7c-43b7-8c41-3bad1b9224ff"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("271c276c-83cb-4396-99a2-2a5570b90170"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("099158a0-81d8-4113-87d9-4f730e051a1a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2fb29065-264a-4275-b6ca-9a40fb876759"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("54836d03-cf47-4617-a9a7-41d354413346"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4422b5a6-0790-4ba7-85e3-02e5db8984bf"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c1f8f96a-6d8e-4b15-aa5f-c611495f4bc9"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("00443259-8179-4695-aa3d-e9f2f411c8b8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8f8a67fd-9056-46a2-bb6b-2d4f02318c10"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c63530a4-35c6-4562-a2e9-8bd961d781ea"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af674536-6a1f-480d-a7c2-7774546afead"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("897facc1-8657-4c7a-b927-ac6dc8d9ab86"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e209426b-d25b-445d-ab33-7e47e497fbdd"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8fbb5a76-10b4-4ce4-8886-4246336bdb2a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fda34b2c-1ce5-405f-ab96-4750ad99ac44"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("668ff787-7b67-40e4-9bdd-03ecba8899d8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b009dd7c-4953-4919-9632-b3bac6cc1ff9"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9e6e704a-bd87-4b52-90af-07258be0e04d"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("52bced75-d3df-43d3-bcad-612368fa018c"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f5c2326a-d11e-4f47-9980-bb64859fd7d9"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("87c28758-9bab-43cd-87e0-71f923c16a44"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("053475d4-5238-4805-9857-4c8c0ed1153a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("12e25983-cea4-48a7-9f7e-9a8891dcba97"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dd177a42-6c2e-4885-9841-82644256651a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25dcaf5b-e0b9-44e0-85f7-957da7798954"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1e87aa59-4703-4a8f-94a4-d8b7c874a060"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("91a66eca-2aea-4837-8b8e-87cda9594ca4"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1fb3000-d26f-4872-b2a2-9654f0b85150"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0f0155b-ac6e-40ff-b8fd-e22d0d2a69ca"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25503f74-8750-4445-a023-971485537085"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bac12225-30ee-44a6-855b-c237314fa2e8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d8999f67-cbee-46cc-9fda-2b3f049a8386"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4ae77ba5-8564-4585-8611-d23d00888e60"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("90a55fa0-e340-42c1-bc2d-a84e8586739a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8aead14e-f328-4d30-9c8f-40109f2957a7"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("29f1118f-1d08-44df-a72d-f8fde49d45f5"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8e380792-56d8-4355-90d1-49509d6491ff"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("755ad2a7-f096-4993-8ce7-de98a0d44490"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("482b180e-811d-44d0-bce7-8fb2d581d9dd"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("04b1127e-a11b-48a9-a346-75a1cc694cb2"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("358026a5-6c9d-46a3-b7be-27b96f62fe69"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ca2b3b2-7a27-46fe-8c6f-aee488aeea1f"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4b422f00-21f6-4369-82ad-001c7c58adba"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b3b81fd0-42bb-41c3-af63-bdaf8341b7f2"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("272d2704-9dcd-49b6-8f25-cd72f686d2a5"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7e3c8ba3-0832-4dbe-b029-6d91273dba60"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1a00773d-404e-4af9-ac46-f8b4bb56e1f3"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d3f8f971-83b8-475b-b96b-73ad397d2a91"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9a1f85f8-f606-4aac-8c55-8033f484916e"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("77cec88c-4dcc-4b25-8eeb-ae9c29abed25"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4725dd10-29fc-4e5d-b38d-38f8946c1445"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f98dcb5c-4529-4d24-b360-fe9c9c495b02"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1206d37-92e5-483e-bd23-30d4b30f6951"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1f07524d-9abf-433d-a0ed-33e089ce2791"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fa52d9c3-c7f5-450a-bab0-5d78222df447"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bb1072b7-7cc9-4259-aba4-136961820fa4"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0bf4405a-6b46-4da1-b730-57abacf02ce2"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca61ffc9-5f89-4ca1-9031-295bb987ad47"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("add6f03b-951f-45ff-bde6-8488bfa58bc7"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d73c1333-adc1-47a1-9141-dccf13ba1a6a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("22bc4a0b-0967-4847-a073-b1c2ef9d3c9c"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a8465b7-4ecb-4c34-b6a8-9de0dc0a73c5"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bc5c1c6e-1808-4ea9-8431-56708457d26b"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d7f8a03-2d57-49fe-873d-8e3126da2c27"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f360d8c2-e52f-4d44-8050-5b9b9cf0bb9e"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21546512-4fb6-43fd-91ad-dd7b342d9922"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("301d5e01-b81f-492e-90c0-125a4c58ac10"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("70e4e2d8-40dc-4dec-84e9-ed3b9f774fd8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ab9d2a8c-3abb-460f-810e-3ace5ef5bf36"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0f75e577-cd92-4bc3-b09a-1a2248d259c8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3df92484-5cc9-4a90-a70f-6244714e7286"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c83e8344-bd21-472d-8e19-153e810455fd"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("30dd9542-a329-4082-b81e-ae625c279ece"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3388b42f-51b3-4f6c-9ef8-8d73f1c83da0"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ef96876-59d7-4bfd-8e11-0c94ce25b0c4"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bb1bb972-df84-4b51-b30f-6c0e84141bc7"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d2e2836e-e1fb-400b-a64e-f2d5c45c9cbb"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("015dcbc0-719e-44a3-80d9-49491352c659"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2af5baf4-87cf-4bc6-ad75-d502b7ba4ee2"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("45fdb8eb-e4b3-41e1-9caa-7eb23a2cc28d"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("547807c9-b4c3-461e-9c8c-e426f2c3f096"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("004e4d01-d163-468f-9e2b-67198f6e9291"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("10ce7a47-0059-4c9d-bbde-3a5c725289c9"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aad1ee27-017b-40cc-84db-4708869f9d87"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("93ee28f7-e23f-4cc1-a10f-ee48dd1fa1a4"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("13a18711-f791-4773-acfc-03fcaeffd4c8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3dffe787-4e27-4215-bccb-18dde5f6624e"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });
        }
    }
}
