# Melodee Localization Expansion — Next 10 Languages (Requirements)

## Purpose
Expand Melodee’s Blazor (.NET 10) UI localization by adding the next **10** highest-impact languages (broad reach + strong web adoption) as additional `.resx` resource files, and ensure each language is fully translatable and quality-checked.

## Current languages (already supported)
- ar-SA, zh-CN, en-US (base `SharedResources.resx`), fr-FR, de-DE, it-IT, ja-JP, pt-BR, ru-RU, es-ES

## Proposed next 10 languages to adopt
These are selected to maximize likely adoption and coverage among common web languages and large online communities.

| Priority | Language | Culture Code | Resource File |
|---:|---|---|---|
| 1 | Dutch (Netherlands) | nl-NL | `SharedResources.nl-NL.resx` |
| 2 | Polish (Poland) | pl-PL | `SharedResources.pl-PL.resx` |
| 3 | Turkish (Turkey) | tr-TR | `SharedResources.tr-TR.resx` |
| 4 | Indonesian (Indonesia) | id-ID | `SharedResources.id-ID.resx` |
| 5 | Korean (Korea) | ko-KR | `SharedResources.ko-KR.resx` |
| 6 | Vietnamese (Vietnam) | vi-VN | `SharedResources.vi-VN.resx` |
| 7 | Persian / Farsi (Iran) *(RTL)* | fa-IR | `SharedResources.fa-IR.resx` |
| 8 | Ukrainian (Ukraine) | uk-UA | `SharedResources.uk-UA.resx` |
| 9 | Czech (Czechia) | cs-CZ | `SharedResources.cs-CZ.resx` |
| 10 | Swedish (Sweden) | sv-SE | `SharedResources.sv-SE.resx` |

> Note: If you want a “large speaker base” alternative swap, consider replacing **sv-SE** or **cs-CZ** with **hi-IN** (Hindi) depending on your target audience.

---

## Scope
### In scope
1. Add the 10 new culture-specific `.resx` files under:
    - `src/Melodee.Blazor/Resources/`
2. Ensure the app recognizes these cultures as supported (culture selection + fallback behavior).
3. Ensure each resource file can be translated to completion with **no runtime formatting errors**.
4. Add automated checks so future English string additions are detectable as “missing translations”.

### Out of scope (for this doc)
- Automatic user-language detection logic beyond standard ASP.NET Core localization
- Non-UI translations (e.g., music metadata localization)

---

## Functional requirements
### R1 — Resource file creation
- For each proposed culture code, a corresponding resource file **must exist** using the naming convention:
    - `SharedResources.<culture>.resx`
- Each file **must include** a full set of keys present in `SharedResources.resx`.

### R2 — Translation completeness tracking
- It must be possible to determine translation status per language:
    - **Missing key** (not present in target `.resx`)
    - **Untranslated** (value is empty or equal to English, if you choose that heuristic)
    - **Translated**
- A CI job must surface failures/warnings when:
    - A key exists in English but not in a target language file
    - A target translation has invalid formatting tokens (see R3)

### R3 — Token/format safety
- Translations must preserve any formatting tokens exactly:
    - numeric placeholders: `{0}`, `{1}`, …
    - named placeholders (if used): `{Name}`, etc.
- Translations must preserve important markup tokens you use in strings (if any), and must not break HTML/Blazor rendering.
- CI must fail on placeholder mismatches between English and target language.

### R4 — RTL support (fa-IR)
- The UI must render correctly for RTL languages already supported (ar-SA) **and** for the new RTL language (fa-IR):
    - Text direction, alignment, icon mirroring (where applicable)
- RTL verification must be documented as a manual QA step (see QA section).

### R5 — Culture fallback behavior
- If a culture-specific resource is missing a key, the app should fall back to:
    1. `SharedResources.resx` (English base), or
    2. Optional: neutral culture fallback if you adopt it later (e.g., `pt` then `pt-BR`)
- Fallback behavior must be consistent and tested (smoke test is fine).

---

## Non-functional requirements
### NFR1 — Maintainability
- Adding a new language in the future should be “repeatable”:
    - add `.resx`
    - register culture
    - run validation
    - translate

### NFR2 — Automation and PR friendliness
- Translation updates should be deliverable as normal GitHub PRs.
- The repo should support external contributors providing translation PRs.

---

## Suggested translation workflow (implementation-agnostic)
Use a continuous localization workflow that:
1. Treats `SharedResources.resx` as canonical (English source of truth).
2. Exposes missing keys and “needs translation” status per language.
3. Produces PRs/commits back to GitHub for translated `.resx` changes.

(Examples: Weblate, Crowdin, Transifex, or a GitHub-only PR workflow.)

---

## QA / Acceptance criteria
A language is considered “adopted” when all criteria below pass:

1. **Files exist** for all 10 languages (R1).
2. **Key parity:** each target `.resx` contains all keys present in `SharedResources.resx` (R1, R2).
3. **Token validation passes**: no placeholder mismatches across all strings (R3).
4. **App smoke test:** UI loads and can switch to each culture without exceptions.
5. **RTL smoke test (fa-IR):**
    - layout is readable, no broken nav/header alignment
    - key UI surfaces (nav, dialogs, forms, labels) display correctly

---

## Deliverables
- 10 new `.resx` files committed to `src/Melodee.Blazor/Resources/`
- Updated supported culture configuration (if not auto-discovered)
- CI validation for key parity + placeholder/token mismatches
- Short contributor note (optional): “How to contribute translations”
