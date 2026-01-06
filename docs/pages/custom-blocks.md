---
title: Custom Blocks
permalink: /custom-blocks/
---

# Custom Blocks

Custom Blocks allow you to inject custom HTML content into specific locations (slots) on Melodee pages. This feature enables you to add announcements, alerts, custom styling, tracking scripts, or any other HTML content without modifying the application code.

## Overview

Custom Blocks are markdown files stored in the Templates library that get rendered as HTML and injected into designated page slots. They support:

- Full HTML and markdown content
- Per-page customization
- Multiple slots per page
- Caching for performance
- Hot-reload during development

## Quick Start

1. **Ensure Custom Blocks are enabled** in `appsettings.json`:
   ```json
   "CustomBlocks": {
     "Enabled": true,
     "MaxBytes": 10240,
     "CacheSeconds": 30
   }
   ```

2. **Create a custom block file** in the Templates library:
   ```
   /storage/templates/custom-blocks/{page}/{slot}.md
   ```

3. **Add content** to the file (markdown or HTML)

4. **Refresh the page** to see your custom block

## File Structure

Custom blocks are stored in the Templates library under the `custom-blocks/` subdirectory:

```
/storage/templates/
└── custom-blocks/
    ├── login/
    │   ├── top.md
    │   └── bottom.md
    ├── register/
    │   └── top.md
    ├── dashboard/
    │   └── announcement.md
    └── _global/
        └── header.md
```

### Naming Convention

Files follow the pattern: `{page}/{slot}.md`

- **Page**: The page identifier (e.g., `login`, `register`, `dashboard`)
- **Slot**: The slot identifier where content should appear (e.g., `top`, `bottom`, `announcement`)

## Available Pages and Slots

### Authentication Pages

**Login Page** (`login`)
- `top` - Above the login form
- `bottom` - Below the login form

**Register Page** (`register`)
- `top` - Above the registration form
- `bottom` - Below the registration form

**Forgot Password Page** (`forgot-password`)
- `top` - Above the forgot password form
- `bottom` - Below the form

**Reset Password Page** (`reset-password`)
- `top` - Above the reset password form
- `bottom` - Below the form

### Application Pages

**Dashboard** (`dashboard`)
- `top` - Above the dashboard content
- `announcement` - Announcement banner
- `bottom` - Below the dashboard content

### Adding New Slots

To add custom blocks to other pages:

1. Add the `<CustomBlock>` component to the page
2. Specify the page and slot names
3. Create the corresponding markdown file

Example in a Razor page:
```razor
<CustomBlock Page="mypage" Slot="top" />
```

Then create: `/storage/templates/custom-blocks/mypage/top.md`

## Content Format

Custom blocks support both Markdown and HTML:

### Markdown Example

```markdown
## Welcome to Melodee!

This is a **custom announcement** for all users.

- Feature 1 is now available
- Check out the new [documentation](/docs)
```

### HTML Example

```html
<div class="alert alert-info">
    <strong>Maintenance Notice:</strong> 
    System maintenance scheduled for Saturday at 2 AM UTC.
</div>

<script>
    console.log('Custom block loaded');
</script>
```

### Bootstrap Alerts

Melodee uses Radzen components, but you can use standard Bootstrap-style classes:

```html
<div class="alert alert-success">
    ✅ New feature released!
</div>

<div class="alert alert-warning">
    ⚠️ Please update your profile information.
</div>

<div class="alert alert-danger">
    🚨 Critical security update required.
</div>
```

## Configuration

Configure Custom Blocks in `appsettings.json`:

```json
{
  "CustomBlocks": {
    "Enabled": true,
    "MaxBytes": 10240,
    "CacheSeconds": 30
  }
}
```

### Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Enabled` | boolean | `true` | Enable/disable Custom Blocks feature |
| `MaxBytes` | integer | `10240` | Maximum file size in bytes (10KB default) |
| `CacheSeconds` | integer | `30` | How long to cache blocks in memory |

### Templates Library Path

Custom blocks are stored in the Templates library. By default, this is `/storage/templates/`. 

To change the Templates library path:

1. Go to **Settings** → **Libraries**
2. Edit the **Templates** library
3. Update the **Path** field
4. Custom blocks will be read from `{Path}/custom-blocks/`

## Caching Behavior

Custom blocks are cached in memory to improve performance:

- **Cache Duration**: Configured by `CacheSeconds` (default 30 seconds)
- **Cache Key**: Based on page and slot name
- **Not Found**: Files that don't exist are NOT cached, so newly created files appear immediately
- **Updates**: Modified files will appear after the cache expires (wait 30 seconds or restart the application)

### Development Mode

During development, set `CacheSeconds` to `0` to disable caching:

```json
"CustomBlocks": {
  "CacheSeconds": 0
}
```

This ensures you see changes immediately without waiting for cache expiration.

## Examples

### Login Page Announcement

**File**: `/storage/templates/custom-blocks/login/top.md`

```markdown
## 🎉 Welcome Back!

We've recently updated our platform with exciting new features:

- **New Music Discovery**: AI-powered recommendations
- **Enhanced Search**: Find music faster than ever
- **Mobile App**: Now available on iOS and Android

[Learn More →](/changelog)
```

### Maintenance Banner

**File**: `/storage/templates/custom-blocks/dashboard/announcement.md`

```html
<div style="background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 4px; margin-bottom: 20px;">
    <strong>⚠️ Scheduled Maintenance:</strong>
    The system will undergo maintenance on <strong>Saturday, January 15th at 2:00 AM UTC</strong>. 
    Expected downtime: 2 hours.
</div>
```

### Terms and Conditions Notice

**File**: `/storage/templates/custom-blocks/register/bottom.md`

```markdown
---

By registering, you agree to our [Terms of Service](/terms) and [Privacy Policy](/privacy).

For questions, contact support@melodee.org
```

### Analytics Script

**File**: `/storage/templates/custom-blocks/dashboard/bottom.md`

```html
<script>
  // Custom analytics
  if (typeof gtag !== 'undefined') {
    gtag('event', 'page_view', {
      page_title: 'Dashboard',
      page_location: window.location.href
    });
  }
</script>
```

## Security Considerations

⚠️ **Important Security Notes:**

1. **Trusted Content Only**: Custom blocks render raw HTML. Only allow trusted administrators to create/edit these files.

2. **No Input Validation**: Content is rendered as-is without sanitization. Malicious scripts can execute.

3. **File System Access**: The Templates library directory should have restricted permissions.

4. **Size Limits**: The `MaxBytes` setting prevents extremely large files, but doesn't validate content.

**Recommendation**: Only enable Custom Blocks if you trust all users with access to the Templates library directory.

## Troubleshooting

### Custom Block Not Appearing

1. **Check if feature is enabled**:
   - Verify `CustomBlocks.Enabled` is `true` in `appsettings.json`

2. **Verify file location**:
   - Ensure file is in `/storage/templates/custom-blocks/{page}/{slot}.md`
   - Check Templates library path in Settings → Libraries

3. **Check file name**:
   - File name must exactly match the page and slot names
   - Names are case-sensitive
   - Must have `.md` extension

4. **Wait for cache expiration**:
   - Default cache is 30 seconds
   - Wait or restart the application
   - Set `CacheSeconds` to `0` during development

5. **Check file size**:
   - File must be under `MaxBytes` limit (default 10KB)
   - Large files are rejected silently

6. **Check logs**:
   - Application logs will show errors reading or parsing files
   - Look for "CustomBlock" in log entries

### File Size Too Large

If you get an error about file size:

1. Increase `MaxBytes` in configuration:
   ```json
   "CustomBlocks": {
     "MaxBytes": 51200
   }
   ```

2. Or reduce your content size

### Content Not Updating

If changes don't appear:

1. **Wait for cache to expire** (default 30 seconds)
2. **Restart the application** to clear cache immediately
3. **Check file timestamp** - ensure the file was actually saved
4. **Set cache to 0** during development for instant updates

### Templates Library Path Issues

If blocks aren't loading, verify the Templates library:

1. Go to **Settings** → **Libraries**
2. Find the **Templates** library (Type: Templates)
3. Verify the **Path** is correct
4. Check the path exists and has read permissions
5. Custom blocks should be under `{Path}/custom-blocks/`

## FAQ

### Where are custom blocks stored?

Custom blocks are stored in the Templates library under the `custom-blocks/` subdirectory. By default, this is `/storage/templates/custom-blocks/`.

### How do I change the storage location?

Update the Templates library path in **Settings** → **Libraries**. Custom blocks will be read from `{NewPath}/custom-blocks/`.

### Can I use custom CSS?

Yes! You can include `<style>` tags or link to external stylesheets:

```html
<style>
  .my-custom-banner {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 20px;
    border-radius: 8px;
  }
</style>

<div class="my-custom-banner">
  Welcome to Melodee!
</div>
```

### Can I use JavaScript?

Yes, you can include `<script>` tags. However, ensure you trust the content source as scripts execute with full page access.

### How do I add blocks to new pages?

1. Add the `<CustomBlock>` component to your Razor page:
   ```razor
   <CustomBlock Page="mypage" Slot="myslot" />
   ```

2. Create the file:
   ```
   /storage/templates/custom-blocks/mypage/myslot.md
   ```

### Can I disable Custom Blocks entirely?

Yes, set `Enabled` to `false` in `appsettings.json`:

```json
"CustomBlocks": {
  "Enabled": false
}
```

### How does caching work?

- Custom blocks are cached in memory for `CacheSeconds` (default 30)
- Cache key is based on page + slot name
- Files that don't exist are NOT cached (new files appear immediately)
- Existing files update after cache expires
- Set `CacheSeconds` to `0` to disable caching (development only)

### What happens if a file doesn't exist?

Nothing. The slot simply remains empty. No error is shown to users. Check application logs if you expect content to appear.

### Can I have global blocks that appear on all pages?

Not directly. You would need to add the `<CustomBlock>` component to each page where you want global content.

Alternatively, you could:
1. Create a shared layout component
2. Add the `<CustomBlock>` there
3. Use a `_global` page name convention
4. Include that component in all pages

## Best Practices

1. **Version Control**: Keep custom block files in version control separate from the Templates library
2. **Backup**: Regularly backup your custom blocks
3. **Testing**: Test custom blocks in a development environment first
4. **Performance**: Keep blocks small and minimize JavaScript
5. **Accessibility**: Ensure custom content is accessible (proper heading hierarchy, alt text, etc.)
6. **Documentation**: Document what each custom block does and why it exists
7. **Review Process**: Require review before deploying custom blocks to production
8. **Monitoring**: Monitor application logs for errors related to custom blocks

## Related Documentation

- [Templates Library](/libraries/) - Managing the Templates library
- [Configuration](/configuration/) - Application configuration options
- [Component Development](/developers/components/) - Creating new Razor components
