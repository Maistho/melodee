using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddMpvMpdSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1901,
                column: "Comment",
                value: "The type of backend to use for jukebox playback (e.g., 'mpv', 'mpd'). Leave empty for no backend.");

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1910, new Guid("e39d8312-cae1-ee40-266d-533077dbfdbb"), 16, "Path to the MPV executable. Leave empty to use system PATH.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpv.path", null, null, 0, null, "" },
                    { 1911, new Guid("945df58f-0546-2e6c-ccc8-210b41e719b7"), 16, "Audio device to use for MPV playback. Leave empty for default device.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpv.audioDevice", null, null, 0, null, "" },
                    { 1912, new Guid("7b99ed1d-9c95-3a2a-9aa7-aca68cda0223"), 16, "Extra command-line arguments to pass to MPV.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpv.extraArgs", null, null, 0, null, "" },
                    { 1913, new Guid("45dfa023-d926-4364-33d1-245a9623dece"), 16, "Path for the MPV IPC socket. Leave empty for auto temp directory.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpv.socketPath", null, null, 0, null, "" },
                    { 1914, new Guid("ac4199ff-57a6-9ded-7a8b-037b9df29a7f"), 16, "Initial volume level for MPV (0.0 to 1.0). Default is 0.8.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpv.initialVolume", null, null, 0, null, "0.8" },
                    { 1915, new Guid("7893e826-0cc8-a0a2-12dc-5c2556212c4a"), 16, "Enable verbose debug output for MPV.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpv.enableDebugOutput", null, null, 0, null, "false" },
                    { 1920, new Guid("bfcce639-8b21-dcc7-b54f-ce1d3ad074f0"), 16, "Unique name/identifier for this MPD instance (for multi-instance support).", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpd.instanceName", null, null, 0, null, "" },
                    { 1921, new Guid("275a59ef-fe5d-c2b8-28df-a7bc4a04abdb"), 16, "Hostname or IP address of the MPD server.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpd.host", null, null, 0, null, "localhost" },
                    { 1922, new Guid("515116f0-99ba-30cc-4b18-d722da60cd7f"), 16, "Port number for MPD connection.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpd.port", null, null, 0, null, "6600" },
                    { 1923, new Guid("dbc39d88-00c0-0710-201e-dd387d745589"), 16, "Password for MPD authentication. Leave empty if no password.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpd.password", null, null, 0, null, "" },
                    { 1924, new Guid("d1d4df5f-fb55-011e-ad6a-c29db5896073"), 16, "Timeout for MPD TCP connection and operations in milliseconds.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpd.timeoutMs", null, null, 0, null, "10000" },
                    { 1925, new Guid("416030fd-3e69-d30e-789f-9203464ebc86"), 16, "Initial volume level for MPD (0.0 to 1.0). Default is 0.8.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpd.initialVolume", null, null, 0, null, "0.8" },
                    { 1926, new Guid("5819d3ec-0b14-1731-2179-69ab1328140b"), 16, "Enable debug logging for MPD commands.", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "mpd.enableDebugOutput", null, null, 0, null, "false" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1910);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1911);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1912);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1913);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1914);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1915);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1920);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1921);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1922);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1923);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1924);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1925);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1926);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1901,
                column: "Comment",
                value: "The type of backend to use for jukebox playback (e.g., 'mpv'). Leave empty for no backend.");
        }
    }
}
