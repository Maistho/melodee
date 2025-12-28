## Goal

Implement SMTP email sending for password reset in the `Melodee.Blazor` solution.

## Current state (already implemented in this repo)

### Backend endpoints already exist

`src/Melodee.Blazor/Controllers/Melodee/AuthController.cs` already provides a token-based reset flow:

- `POST /api/v1/auth/password-reset/request` (anonymous, rate limited)
  - Calls `UserService.GeneratePasswordResetTokenAsync(email)`.
  - Currently returns a generic message *and* (for now) includes `resetToken` and `resetUrl` in the response.
- `GET /api/v1/auth/password-reset/validate/{token}` (anonymous)
  - Validates token without consuming it.
- `POST /api/v1/auth/password-reset/confirm` (anonymous, rate limited)
  - Resets password and consumes token.

### Token storage + expiry already exist

`src/Melodee.Common/Services/UserService.cs`:

- Generates a 32-byte cryptographically-random token and encodes it as Base64URL (replacing `+`/`/` and trimming `=`), which is safe for use in a query string.
- Stores token in DB fields on `Users`:
  - `PasswordResetToken`
  - `PasswordResetTokenExpiresAt` (**must be configurable**, default 60 minutes; see `security.passwordResetTokenExpiryMinutes`)

### UI has a placeholder

`src/Melodee.Blazor/Components/Pages/Account/Login.razor` has a localized “Forgot password” link, but `OnForgotPassword()` is TODO.

## Recommended approach for Melodee.Blazor

1. Keep the existing token + expiry implementation (it already meets the security requirements and does not require ASP.NET Core Identity UI).
2. Add an SMTP sender service (MailKit).
3. Implement the **Blazor UI workflow using DI-injected services directly** (no calls to the Melodee external-client REST API controllers):
   - Collect an email address (forgot password)
   - Generate token via `UserService.GeneratePasswordResetTokenAsync(email)`
   - Send email via `IEmailSender` using `{system.baseUrl}/reset-password?token=...`
   - Accept token + new password (reset password)
   - Reset password via `UserService.ResetPasswordWithTokenAsync(token, newPassword)`

> Note: The REST endpoints in `Melodee.Blazor.Controllers.Melodee` remain for external clients and should not be used by the server-side Blazor pages.

## Security requirements (must keep)

- **No account enumeration:** always return the same generic response for request/reset initiation (already done in the controller).
- **Do not return token/resetUrl in production responses:**
  - Keep returning only the generic message.
  - Optionally include `resetUrl` only when `IHostEnvironment.IsDevelopment()` to aid local testing.
  - Even in development, treat reset URLs/tokens as sensitive: never log them, never show them in error UI, and avoid sharing screenshots/logs that contain them.
- **Do not log secrets or tokens.** If logging email, use masking (see `LogSanitizer.MaskEmail`).
- **HTTPS required** for reset links in production.
- **API rate limiting** remains enabled on the request/confirm endpoints (for external clients).
- **Blazor-side rate limiting is required:** implement simple cache-based rate limiting for the Blazor form actions to reduce abuse/accidental spam.
  - Minimum: throttle per-client/session for forgot-password submit and reset-password submit (e.g., 3 attempts per 10 minutes).
  - Implementation guidance:
    - Use `IMemoryCache` by default (simple single-instance). If you run multiple app instances, switch to a distributed cache (e.g., Redis) for consistent limiting.
    - Keying: prefer a stable server-known identifier (session id / circuit id) and optionally include client IP if available via `IHttpContextAccessor`.
    - Use a fixed window (e.g., 10 minutes) with an incrementing counter; a sliding window is optional.
  - Must not leak whether an email exists.

## Reset link construction (important details)

### URL construction details

- Use `system.baseUrl` (SettingRegistry: `SettingRegistry.SystemBaseUrl`) as the canonical public URL for emails.
  - `BaseUrlService` and other code already follow this pattern.
  - Fallback to `Request.Scheme/Request.Host` only if `system.baseUrl` is not configured.
- Reset URL should match a Blazor page route you implement:
  - `GET {baseUrl}/reset-password?token={token}`
- Always `Uri.EscapeDataString(token)` when building the query string.

## SMTP configuration (must use SettingService)

This codebase’s settings system uses **dot-separated keys** (not `email:FromName`) and can be overridden by environment variables.

### Required settings keys (add to SettingRegistry + seed defaults)

- `email.enabled` (bool; default false)
- `email.fromName` (string)
- `email.fromEmail` (string)
- `email.smtpHost` (string)
- `email.smtpPort` (int)
- `email.smtpUsername` (string; optional)
- `email.smtpPassword` (string; optional)
- `email.smtpUseSsl` (bool)
- `email.smtpUseStartTls` (bool)
- `security.passwordResetTokenExpiryMinutes` (int; default 60)

Optional but useful:

- `email.resetPassword.subject`
- `email.resetPassword.textBodyTemplate`
- `email.resetPassword.htmlBodyTemplate`

### Environment variable override rules (important)

This repository automatically maps env vars to settings by replacing `_` with `.`.

Examples:

- Setting key `system.baseUrl` can be set via env var `system_baseUrl`
- Setting key `email.smtpPassword` can be set via env var `email_smtpPassword`

Recommendation: **set `email.smtpPassword` via environment variable** (to avoid storing secrets in the DB), but still treat it as a “SettingService key” from the application’s perspective.

## Email content requirements

- Send both **text** and **HTML** versions.
- Templates may be stored in settings; if so, support simple variable substitution at minimum:
  - `{resetUrl}`
  - `{expiryMinutes}`
  - (optional) `{appName}` / `{baseUrl}`
- Include:
  - A short explanation
  - The reset link
  - Expiration note (configured token expiry; default 60 minutes)
  - A “ignore this email” line

Example subject/body (non-localized server-side template is acceptable initially):

- Subject: `Reset your Melodee password`
- Text body:
  - `Someone requested a password reset for your Melodee account.`
  - `Reset your password using this link (valid for {expiryMinutes} minutes): {resetUrl}`
  - `If you didn't request this, you can ignore this email. Your account is safe and your password has not been changed.`
  - `---`
  - `This email was sent from {system.baseUrl}.`

## UI work (minimal)

### Forgot password UX (Blazor UI)

- Add a page or dialog that prompts for **email**.
- Validate input before attempting the workflow:
  - Reject obviously invalid email formats (basic syntax validation) but still return a generic message (no enumeration).
- Use DI services directly (no HTTP):
  - `UserService.GeneratePasswordResetTokenAsync(email)`
  - If a token is returned and `email.enabled` is true, build the reset URL and call `IEmailSender.SendAsync(...)`.
- Always show the same generic success message regardless of whether the user exists or whether email sending is enabled.

### Reset password UX (Blazor UI)

- Add a page at `/reset-password` that:
  - Reads `token` from query string
  - **Required:** validate token using DI during page lifecycle (before allowing password submission):
    - `UserService.ValidatePasswordResetTokenAsync(token)`
    - If invalid/expired, show an error and do not render/enable the reset form.
  - On submit, reset password using DI:
    - `UserService.ResetPasswordWithTokenAsync(token, newPassword)`

## Email sender abstraction (`IEmailSender`) clarification

`IEmailSender` is **not** a .NET framework built-in interface.

- If you use ASP.NET Core Identity UI, there is an interface named `Microsoft.AspNetCore.Identity.UI.Services.IEmailSender`, but this Melodee solution is not using Identity UI for the password reset flow.
- For Melodee, **create a project-local interface** (recommended namespace: `Melodee.Blazor.Services.Email.IEmailSender`) so Blazor pages can depend on it without taking an Identity UI dependency.
- Suggested contract:
  - `Task SendAsync(string toEmail, string subject, string textBody, string? htmlBody, CancellationToken cancellationToken = default)`

## MailKit implementation notes

- Use `MailKit.Net.Smtp.SmtpClient` (not `System.Net.Mail`).
- Use async APIs (`ConnectAsync`, `AuthenticateAsync`, `SendAsync`, `DisconnectAsync`) to avoid blocking in server-side Blazor.
- Support SSL/StartTLS based on the two bool settings.
- Authenticate only when username/password are provided.
- Error handling requirements:
  - Never surface SMTP-specific errors to end users (avoid information disclosure); show a generic “If the account exists, you’ll receive an email” message.
  - Log SMTP failures with safe context (masked recipient email, exception type/message) but never log tokens or SMTP passwords.
  - Consider emitting an admin-facing log/event for repeated failures (operational alerting).

## Operational fallback & monitoring

- If `email.enabled` is false or SMTP is misconfigured:
  - The UI must still respond with the same generic message.
  - Log a warning (no tokens) so admins can detect misconfiguration.
  - No alternative reset mechanism should be exposed to end users (to avoid creating an insecure bypass).
- Track password reset activity for security monitoring:
  - Log (masked) email + timestamp + whether rate limiting was triggered.
  - Optionally persist an audit entry if the system has an audit/event log pattern.

## Internationalization (future)

Non-localized templates are acceptable initially, but plan for **fully localized email templates**.

Practical approach:

- Maintain separate templates per language (subject + text + html) for the reset-password email.
- Template selection order:
  1. User `PreferredLanguage` (best)
  2. Current UI culture (acceptable fallback)
  3. Default template (en-US)
- Do not attempt to "live translate" content at send time; use explicit translated templates.
- Start with only en-US, but design the lookup so adding additional languages is data/config only (no code changes).

## Manual test plan

1. Configure `system.baseUrl` and SMTP settings (DB settings or env vars).
2. Start `Melodee.Blazor` and click “Forgot password” on the login page.
3. Verify the email is received with a valid link.
4. Open the link, set a new password, confirm success.
5. Verify token can’t be reused and expires after the configured TTL (default 60 minutes).

## Example curl (external-client API only)

These endpoints exist for external clients; the Blazor UI should not use them.

Request reset:

```bash
curl -i -X POST "https://localhost:5001/api/v1/auth/password-reset/request" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com"}'
```

Validate token:

```bash
curl -i "https://localhost:5001/api/v1/auth/password-reset/validate/{token}"
```

Confirm reset:

```bash
curl -i -X POST "https://localhost:5001/api/v1/auth/password-reset/confirm" \
  -H "Content-Type: application/json" \
  -d '{"token":"{token}","newPassword":"NewPassword123!"}'
```

## Automated test plan (optional)

This repo does not currently include Playwright; avoid adding a new E2E framework just for this feature.

Preferred: add tests to the existing test projects (xUnit) to cover:

1. Token generation sets `PasswordResetTokenExpiresAt` based on `security.passwordResetTokenExpiryMinutes`.
2. Token validation failures:
   - invalid token
   - expired token
3. Reset consumes the token (cannot be reused) and updates the password.
4. Forgot-password email send behavior:
   - mock `IEmailSender` (do not hit real SMTP in tests)
   - verify it is called only when a token is generated
5. Blazor-side rate limiting blocks repeated submissions.

## Expected output from implementation

- List of files added/changed with one-line explanation per file.
- Setting keys + recommended env var names.
- A short click-path test plan (login → forgot password → email → reset page).
