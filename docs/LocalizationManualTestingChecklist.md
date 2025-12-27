# Melodee.Blazor Localization Manual Testing Checklist

## Overview
This checklist guides manual testing of the localization implementation across all 6 supported languages.

## Supported Languages
1. English (en-US) - Default
2. Spanish (es-ES)
3. Russian (ru-RU)
4. Chinese (zh-CN)
5. French (fr-FR)
6. Arabic (ar-SA)

## Pre-Test Setup
- [ ] Build the application: `dotnet build Melodee.sln`
- [ ] Run the application: `dotnet run --project src/Melodee.Blazor/Melodee.Blazor.csproj`
- [ ] Open browser to the application URL
- [ ] Clear browser cache and local storage

## Test Scenarios

### 1. Language Selector Component
- [ ] Language selector appears in the header/navigation
- [ ] All 6 languages are listed in the selector
- [ ] Current language is highlighted
- [ ] Clicking a language changes the UI immediately
- [ ] Language preference persists after page refresh
- [ ] Language preference persists after browser restart (localStorage)

### 2. Dashboard Page (for each language)
- [ ] Page title changes based on language
- [ ] Navigation breadcrumb is localized
- [ ] "Your Pins" section title is localized
- [ ] "Recently Played" section title is localized
- [ ] "Top Played Songs" section title is localized
- [ ] All statistics labels are localized
- [ ] Chart titles are localized

### 3. Artists Page (for each language)
- [ ] Page title: "Artists" / "Artistas" / etc.
- [ ] Breadcrumb: "Dashboard" > "Artists"
- [ ] Statistics panel header is localized
- [ ] Action buttons: "Add", "Merge", "Delete"
- [ ] Data grid column headers: Name, Alt Names, Directory, Albums, Songs, Created, Tags
- [ ] Merge dialog title and content are localized
- [ ] Delete confirmation dialog is localized
- [ ] Lock warning tooltip displays in correct language
- [ ] Notification messages appear in correct language

### 4. Albums Page (for each language)
- [ ] Page title and breadcrumb localized
- [ ] Action buttons localized
- [ ] Column headers localized
- [ ] Status labels localized

### 5. Songs Page (for each language)
- [ ] Page title: "Songs" /  "Canciones" / etc.
- [ ] Breadcrumb localized
- [ ] Statistics panel with "Click to filter" tooltip
- [ ] Action buttons localized
- [ ] Column headers: Title, Song Number, Artist, Album, Duration, File Size, Rating, Loved, Created, Tags
- [ ] Merge dialog localized
- [ ] Filter buttons work ("Your Favorite Songs", "Your Rated Songs")
- [ ] Lock warning message localized

### 6. Playlists Page (for each language)
- [ ] Page title and breadcrumb localized
- [ ] Action buttons localized
- [ ] Column headers: Name, Created, Tags
- [ ] Delete confirmation dialog localized
- [ ] Lock warning message localized

### 7. Profile Page (for each language)
- [ ] Page title: "Your Profile" / "Tu Perfil" / etc.
- [ ] Section headers: "Profile Information", "Linked Accounts"
- [ ] Form labels: Username, Email, Bio, Time Zone
- [ ] Profile image section localized
- [ ] Google account linking section:
  - [ ] "Link Google Account" button
  - [ ] "Unlink" button when linked
  - [ ] "Linked" status indicator
- [ ] Save button: "Save Changes" / "Guardar Cambios" / etc.
- [ ] Saving indicator appears in correct language
- [ ] Validation messages:
  - [ ] "Username is required"
  - [ ] "Email is required"
  - [ ] "Invalid email address"
  - [ ] "Invalid timezone: {tz}"
- [ ] Success notifications localized
- [ ] Error notifications localized
- [ ] Unlink confirmation dialog localized

### 8. Login Page (for each language)
- [ ] Page title localized
- [ ] Welcome message localized
- [ ] Email and Password field labels
- [ ] "Sign In" button text
- [ ] "Register" link text
- [ ] "Signing in..." loading message
- [ ] Error messages (invalid credentials, etc.)
- [ ] Google Sign-In button text
- [ ] Google auth error messages

### 9. Register Page (for each language)
- [ ] Page title localized
- [ ] All form field labels
- [ ] "Register" button text
- [ ] "Already have an account? Login" link
- [ ] Access code section if applicable
- [ ] Validation error messages
- [ ] Success messages

### 10. About Page (for each language)
- [ ] Page title and breadcrumb localized
- [ ] Server information labels
- [ ] System information section headers
- [ ] Platform, Version, etc. labels
- [ ] API documentation links and descriptions

## Culture-Specific Tests

### Date Formatting
For each language, verify dates display correctly:
- [ ] en-US: MM/DD/YYYY (12/27/2025)
- [ ] es-ES: DD/MM/YYYY (27/12/2025)
- [ ] ru-RU: DD.MM.YYYY (27.12.2025)
- [ ] zh-CN: YYYY/MM/DD (2025/12/27)
- [ ] fr-FR: DD/MM/YYYY (27/12/2025)
- [ ] ar-SA: DD/MM/YYYY (27/12/2025) - RTL

### Number Formatting
For each language, verify numbers display correctly:
- [ ] en-US: 1,234.56
- [ ] es-ES: 1.234,56
- [ ] ru-RU: 1 234,56
- [ ] zh-CN: 1,234.56
- [ ] fr-FR: 1 234,56
- [ ] ar-SA: 1,234.56

### Right-to-Left (RTL) Support
For Arabic (ar-SA):
- [ ] Text direction is right-to-left
- [ ] Navigation menu aligns to the right
- [ ] Form fields align correctly
- [ ] Icons and buttons align correctly
- [ ] Data tables display correctly
- [ ] Dialogs and modals display correctly

## Edge Cases

### Missing Translations
- [ ] If a key is missing, the key name itself displays (not blank)
- [ ] No console errors for missing keys
- [ ] Application continues to function

### Language Switching
- [ ] Switch between all languages multiple times
- [ ] No memory leaks or performance degradation
- [ ] State is preserved when switching languages
- [ ] Forms retain their data when switching languages

### Long Text Handling
Test with languages that have longer translations (German, if added):
- [ ] Buttons don't overflow
- [ ] Menu items don't wrap awkwardly
- [ ] Tables adjust column widths appropriately

## Browser Compatibility
Test in multiple browsers for each language:
- [ ] Chrome/Chromium
- [ ] Firefox
- [ ] Edge
- [ ] Safari (if available)

## Accessibility
- [ ] Screen reader announces content in selected language
- [ ] ARIA labels are localized
- [ ] Keyboard navigation works in all languages
- [ ] Focus indicators are visible in all languages

## Performance
- [ ] Initial page load time is acceptable
- [ ] Language switching is instant (< 500ms)
- [ ] No flickering when changing languages
- [ ] Resource files load efficiently

## Issues Found
Document any issues discovered during testing:

| Issue | Language | Component | Severity | Notes |
|-------|----------|-----------|----------|-------|
|       |          |           |          |       |

## Sign-Off
- **Tester Name**: _______________
- **Date**: _______________
- **Languages Tested**: _______________
- **Overall Result**: PASS / FAIL
- **Comments**: _______________
