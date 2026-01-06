<!-- markdownlint-disable-file -->
# Release Changes: Custom Blocks Feature Implementation

**Related Plan**: `prompts/CUSTOM-BLOCKS.md`
**Implementation Date**: 2025-01-01

## Summary

Implemented the Custom Blocks feature for Melodee, allowing administrators to add custom Markdown content blocks to authentication pages (login, register, forgot-password, reset-password). The implementation follows a security-first approach with strict key validation, HTML sanitization, and automatic caching with file-based invalidation.

## Changes

### Added

- `src/Melodee.Blazor/Services/CustomBlocks/CustomBlocksOptions.cs` - Configuration class
- `src/Melodee.Blazor/Services/CustomBlocks/CustomBlockResult.cs` - Result record  
- `src/Melodee.Blazor/Services/CustomBlocks/ICustomBlockService.cs` - Service interface
- `src/Melodee.Blazor/Services/CustomBlocks/FileCustomBlockService.cs` - Main implementation (166 lines)
- `src/Melodee.Blazor/Services/CustomBlocks/MarkdownRenderer.cs` - Markdig wrapper with DisableHtml()
- `src/Melodee.Blazor/Services/CustomBlocks/HtmlSanitizerService.cs` - HTML sanitizer with strict allow-list
- `src/Melodee.Blazor/Components/CustomBlock.razor` - Blazor rendering component
- `tests/Melodee.Tests.Blazor/Services/CustomBlocks/FileCustomBlockServiceTests.cs` - Test suite (37 tests)

### Modified

- `src/Melodee.Blazor/Components/Pages/Account/Login.razor` - Injected custom block slots
- `src/Melodee.Blazor/Components/Pages/Account/Register.razor` - Injected custom block slots
- `src/Melodee.Blazor/Components/Pages/Account/ForgotPassword.razor` - Injected custom block slots
- `src/Melodee.Blazor/Components/Pages/Account/ResetPassword.razor` - Injected custom block slots
- `src/Melodee.Blazor/appsettings.json` - Added CustomBlocks configuration section
- `src/Melodee.Blazor/Program.cs` - Added DI registration for custom blocks services

## Release Summary

**Total Files Affected**: 13  
**Files Created**: 8  
**Files Modified**: 5  
**Files Removed**: 0

### Security Features
- Key validation regex prevents path traversal
- Max file size 256KB (configurable)
- Markdown DisableHtml() prevents raw HTML
- Strict HTML sanitization allow-list
- Cache invalidation on file timestamp change

### Testing
✅ All 37 unit tests passing  
✅ Solution builds with 0 errors

### Deployment
Requires `MELODEE_DATA_DIR` environment variable for custom blocks directory resolution.
