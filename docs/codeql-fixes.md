# CodeQL Security Fixes

**Date**: 2026-01-02 (Updated)
**Status**: ✅ COMPLETED

## Summary

This document tracks all CodeQL security alerts identified in the Melodee codebase and their remediation status.

## Alert Inventory

### A. Weak Cryptography (cs/weak-crypto)

| Status | File | Line | Root Cause | Fix Strategy |
|--------|------|------|------------|--------------|
| ✅ DOCUMENTED | HashHelper.cs | 24 | MD5 for external API compatibility | Required by OpenSubsonic/Last.fm APIs - cannot change |
| ✅ DOCUMENTED | HashHelper.cs | 44 | MD5 for external API compatibility | Required by OpenSubsonic/Last.fm APIs - cannot change |
| ✅ DOCUMENTED | UserService.cs | ~1064 | MD5 for OpenSubsonic token auth | Required by protocol spec - cannot change |
| ✅ DOCUMENTED | ScrobbleController.cs | ~291 | MD5 for Last.fm API signature | Required by Last.fm API - cannot change |
| ✅ DOCUMENTED | JellyfinControllerBase.cs | 58 | MD5 for server ID generation | Non-cryptographic GUID generation for Jellyfin API |
| ✅ DOCUMENTED | ItemsController.cs | 1084 | MD5 for ETag computation | Non-cryptographic ETag generation for HTTP caching |
| ✅ DOCUMENTED | PlaylistsController.cs | 506 | MD5 for ETag computation | Non-cryptographic ETag generation for HTTP caching |
| ✅ DOCUMENTED | MusicGenresController.cs | 115, 122 | MD5 for genre GUID and ETags | Non-cryptographic ID generation for Jellyfin API |
| ✅ DOCUMENTED | ArtistsController.cs | 299, 306 | MD5 for ETag computation | Non-cryptographic ETag generation for HTTP caching |
| ✅ DOCUMENTED | MelodeeDbContext.cs | 95 | MD5 for seed data GUIDs | Non-cryptographic deterministic GUID generation |

**Note**: MD5 usages are either required by external API specifications or used for non-cryptographic purposes (GUID/ETag generation). All are properly documented with `// lgtm[cs/weak-crypto]` comments explaining the justification.

### B. Regex DoS (cs/regex-injection)

| Status | File | Line | Root Cause | Fix Strategy |
|--------|------|------|------------|--------------|
| ✅ FIXED | ITunesSearchEngine.cs | 319, 325 | `new Regex()` without timeout | Added `TimeSpan.FromSeconds(5)` timeout |
| ✅ FIXED | StringExtensions.cs | 930 | `new Regex()` without timeout | Added `TimeSpan.FromSeconds(5)` timeout |
| ✅ FIXED | ConfigurationListCommand.cs | 34 | `new Regex()` without timeout | Added `TimeSpan.FromSeconds(5)` timeout |

### C. Path Traversal (cs/path-traversal)

| Status | File | Line | Root Cause | Fix Strategy |
|--------|------|------|------------|--------------|
| ✅ FIXED | AlbumDetail.razor | 712 | `file.Name` from upload used in Path.Combine | Use SafePath.ResolveUnderRoot() to sanitize and validate |

### D. Cross-Site Scripting (cs/xss)

| Status | File | Line | Root Cause | Fix Strategy |
|--------|------|------|------------|--------------|
| ✅ FIXED | Markdown.razor | 6 | MarkupString renders unsanitized HTML from user content | Added HtmlSanitizer with allowlist of safe tags/attributes |

## Fix Progress

### Completed Fixes

1. **Regex DoS Prevention** - Added timeouts to all runtime-constructed Regex instances (2025-12-21, 2026-01-02)
2. **Path Traversal Prevention** - Created SafePath utility and used it in file upload handler (2025-12-21)
3. **XSS Prevention** - Integrated HtmlSanitizer for Markdown component (2025-12-21)
4. **MD5 Documentation** - Added comprehensive documentation for all MD5 usages in Jellyfin API controllers and database seeding (2026-01-02)

### Files Modified

**Original fixes (2025-12-21):**
- `src/Melodee.Common/Plugins/SearchEngine/ITunes/ITunesSearchEngine.cs` - Added regex timeouts
- `src/Melodee.Common/Extensions/StringExtensions.cs` - Added regex timeout
- `src/Melodee.Blazor/Components/Pages/Data/AlbumDetail.razor` - Used SafePath for file uploads
- `src/Melodee.Blazor/Components/Components/Markdown.razor` - Added HTML sanitization
- `src/Melodee.Blazor/Melodee.Blazor.csproj` - Added HtmlSanitizer package reference
- `Directory.Packages.props` - Added HtmlSanitizer version

**Additional fixes (2026-01-02):**
- `src/Melodee.Cli/Command/ConfigurationListCommand.cs` - Added regex timeout
- `src/Melodee.Blazor/Controllers/Jellyfin/JellyfinControllerBase.cs` - Added MD5 documentation for server ID generation
- `src/Melodee.Blazor/Controllers/Jellyfin/ItemsController.cs` - Added MD5 documentation for ETag computation
- `src/Melodee.Blazor/Controllers/Jellyfin/PlaylistsController.cs` - Added MD5 documentation for ETag computation
- `src/Melodee.Blazor/Controllers/Jellyfin/MusicGenresController.cs` - Added MD5 documentation for genre GUID and ETag generation
- `src/Melodee.Blazor/Controllers/Jellyfin/ArtistsController.cs` - Added MD5 documentation for ETag computation
- `src/Melodee.Common/Data/MelodeeDbContext.cs` - Added MD5 documentation for seed data GUID generation
- `docs/codeql-fixes.md` - Updated with 2026-01-02 fixes

### Files Created (2025-12-21)

- `src/Melodee.Common/Utility/SafePath.cs` - New security utility for path validation
- `tests/Melodee.Tests.Common/Utility/SafePathTests.cs` - Unit tests for SafePath

## Implementation Details

### Fix B1 & B2: Regex with Timeout

Added `TimeSpan.FromSeconds(5)` timeout to prevent ReDoS attacks:

```csharp
// Before
var regex = new Regex(pattern);

// After
var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(5));
```

### Fix C1: Path Traversal Prevention

Created `SafePath` utility class with:
- `SanitizeFileName()` - Removes path separators and ".." sequences
- `ResolveUnderRoot()` - Combines paths and validates result stays within base directory
- `IsPathWithinBase()` - Checks if a path is confined to a base directory

Usage in file upload:
```csharp
// Before
var target = Path.Combine(dir, file.Name);

// After
var target = SafePath.ResolveUnderRoot(dir, file.Name);
if (target == null) {
    // Invalid filename - reject upload
    continue;
}
```

### Fix D1: XSS Prevention

Added HtmlSanitizer with strict allowlist:

```csharp
private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

private static HtmlSanitizer CreateSanitizer()
{
    var sanitizer = new HtmlSanitizer();
    // Only allow safe HTML tags for markdown content
    sanitizer.AllowedTags.Add("h1"); // headings
    sanitizer.AllowedTags.Add("p");  // paragraphs
    sanitizer.AllowedTags.Add("a");  // links
    // ... other safe tags
    
    // Only allow safe URL schemes
    sanitizer.AllowedSchemes.Add("https");
    sanitizer.AllowedSchemes.Add("http");
    sanitizer.AllowedSchemes.Add("mailto");
    
    return sanitizer;
}
```

## Testing

- [x] Solution builds successfully
- [x] All existing tests pass (175 tests)
- [x] New SafePath tests pass (27 tests)
- [x] No regressions in functionality

## References

- [OWASP Path Traversal](https://owasp.org/www-community/attacks/Path_Traversal)
- [OWASP XSS Prevention](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html)
- [.NET Regex Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/base-types/best-practices)
- [OpenSubsonic API Authentication](http://www.subsonic.org/pages/api.jsp#authentication)
- [Last.fm API Authentication](https://www.last.fm/api/authentication)
