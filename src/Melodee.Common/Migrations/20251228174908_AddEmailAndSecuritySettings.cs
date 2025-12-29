using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAndSecuritySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1500, new Guid("77c527bc-5317-46da-d778-e7114791749f"), null, "Enable or disable email sending functionality", NodaTime.Instant.FromUnixTimeTicks(0L), "When true, enables SMTP email sending for password resets and notifications", false, "email.enabled", null, null, 0, null, "false" },
                    { 1501, new Guid("1836553b-06a0-2fe4-35c0-fdf088520e61"), null, "Display name in From field of outgoing emails", NodaTime.Instant.FromUnixTimeTicks(0L), null, false, "email.fromName", null, null, 0, null, "Melodee" },
                    { 1502, new Guid("28ce7a91-9dd3-bcdb-7cf2-2249037ff4a5"), null, "Email address in From field (REQUIRED for email sending)", NodaTime.Instant.FromUnixTimeTicks(0L), "Example: noreply@yourdomain.com", false, "email.fromEmail", null, null, 0, null, "" },
                    { 1503, new Guid("100f5f84-1a12-8af4-1b43-349bfea18d90"), null, "SMTP server hostname (REQUIRED for email sending)", NodaTime.Instant.FromUnixTimeTicks(0L), "Example: smtp.gmail.com or smtp.sendgrid.net", false, "email.smtpHost", null, null, 0, null, "" },
                    { 1504, new Guid("0f9b5ef0-1b03-2319-7e19-5fc2e9e7287d"), null, "SMTP server port", NodaTime.Instant.FromUnixTimeTicks(0L), "Common values: 587 (StartTLS), 465 (SSL), 25 (unencrypted)", false, "email.smtpPort", null, null, 0, null, "587" },
                    { 1505, new Guid("41c53bd6-7fd6-bd69-673c-e352fa5f84a5"), null, "SMTP authentication username (optional)", NodaTime.Instant.FromUnixTimeTicks(0L), "Leave empty if SMTP server does not require authentication", false, "email.smtpUsername", null, null, 0, null, "" },
                    { 1506, new Guid("893a9053-2b8f-8a32-4e6c-c9b3541341db"), null, "SMTP authentication password (optional, use env var email_smtpPassword)", NodaTime.Instant.FromUnixTimeTicks(0L), "For security, set via environment variable: email_smtpPassword", false, "email.smtpPassword", null, null, 0, null, "" },
                    { 1507, new Guid("9a20a527-a2d9-628f-914a-c2fab2dc8496"), null, "Use SSL connection for SMTP", NodaTime.Instant.FromUnixTimeTicks(0L), "Set to true for port 465 (SSL), false for port 587 (StartTLS)", false, "email.smtpUseSsl", null, null, 0, null, "false" },
                    { 1508, new Guid("1f6249d4-fb89-6266-9672-41d7a6109260"), null, "Use StartTLS for SMTP", NodaTime.Instant.FromUnixTimeTicks(0L), "Recommended: true for port 587", false, "email.smtpUseStartTls", null, null, 0, null, "true" },
                    { 1600, new Guid("f27eb478-3910-50ce-7a05-86aff6d0f1ca"), null, "Password reset token expiry time in minutes", NodaTime.Instant.FromUnixTimeTicks(0L), "How long password reset links remain valid (default: 60 minutes)", false, "security.passwordResetTokenExpiryMinutes", null, null, 0, null, "60" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1500);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1501);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1502);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1503);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1504);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1505);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1506);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1507);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1508);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1600);
        }
    }
}
