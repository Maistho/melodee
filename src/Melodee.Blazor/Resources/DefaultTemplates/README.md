# Templates

Email templates are stored in the Templates library, organized by language code.

## Directory Structure

```
/storage/templates/
├── en-us/
│   ├── PasswordReset.txt
│   └── PasswordReset.html
├── fr-fr/
│   ├── PasswordReset.txt
│   └── PasswordReset.html
└── ... (other languages)
```

## Template Variables

All templates support the following variables:

- `{resetUrl}` - The password reset URL (required)
- `{expiryMinutes}` - Token expiry time in minutes
- `{appName}` - Application name ("Melodee")
- `{baseUrl}` - Base URL of the application

## Creating Templates for New Languages

1. Create a directory with the lowercase language code (e.g., `fr-fr`, `de-de`)
2. Copy the `en-us/PasswordReset.txt` and `PasswordReset.html` files
3. Translate the content, keeping the `{variable}` placeholders intact

## Template Types

### PasswordReset.txt
Plain text version of the password reset email. Used as fallback for email clients that don't support HTML.

### PasswordReset.html
HTML version with styling. This is the primary template used for most email clients.

## Testing Templates

After modifying templates, test by:
1. Requesting a password reset
2. Checking the email in multiple clients (Gmail, Outlook, Apple Mail)
3. Verifying all variables are replaced correctly
4. Testing links work and expire as expected

## Customization

Templates can be customized per installation. The application will:
1. First try to load from the Templates library path
2. Fall back to hardcoded defaults if template files don't exist

This allows for:
- Custom branding per installation
- Localized content
- A/B testing different templates
- Easy updates without redeployment
