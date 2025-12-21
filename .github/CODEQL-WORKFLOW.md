# CodeQL Workflow Configuration

## Current State: Dual CodeQL Setup

This repository currently has **two CodeQL analysis configurations** running:

### 1. Custom CodeQL Workflow (`.github/workflows/codeql.yml`)
- **Location**: `.github/workflows/codeql.yml`
- **Status**: Active, but results NOT uploaded to GitHub Security
- **Configuration**: Custom setup with advanced features
- **Languages**: C# and JavaScript/TypeScript (matrix strategy)
- **Custom Config**: `.github/codeql/codeql-config.yml` (path filtering)
- **Upload Setting**: `upload: false` (to avoid conflicts with default setup)

### 2. GitHub Default Setup
- **Location**: Enabled in repository settings (no visible file)
- **Status**: Active (inferred from workflow comments)
- **Configuration**: GitHub-managed default
- **Languages**: Auto-detected
- **Results**: Uploaded to GitHub Security tab

## Why This Exists

The comment in `codeql.yml` lines 78-80 states:
```yaml
# Prevent workflow failure when GitHub "default setup" code scanning is enabled.
# If you want advanced results uploaded, disable default setup or set upload: true.
upload: false
```

This indicates that both setups were running, causing conflicts. The custom workflow was modified to NOT upload results to avoid workflow failures.

## Problem

Having two CodeQL setups is inefficient and confusing:
- ❌ Duplicate analysis runs (wastes CI/CD minutes)
- ❌ Custom workflow results not visible in Security tab
- ❌ Configuration spread across multiple systems
- ❌ Unclear which analysis results are authoritative

## Recommended Solution: Keep Custom Workflow

**Advantages of the custom workflow:**
1. ✅ **Path filtering** - Excludes docs, vendored JS, benchmarks (via `codeql-config.yml`)
2. ✅ **Explicit language control** - C# and JavaScript/TypeScript
3. ✅ **Version control** - Configuration tracked in Git
4. ✅ **Transparency** - Visible in repository, reviewable in PRs
5. ✅ **Flexibility** - Can add custom queries, modify triggers, adjust timeout
6. ✅ **Already proven** - Working configuration with comprehensive security audit

**Steps to migrate:**

### Step 1: Disable GitHub Default Setup
1. Go to repository **Settings** → **Code security and analysis**
2. Find **Code scanning** section
3. Locate **CodeQL analysis** → **Default setup**
4. Click **Disable** or switch to **Advanced**

> **Note**: You must have admin access to the repository to change these settings.

### Step 2: Enable Uploads in Custom Workflow
Update `.github/workflows/codeql.yml` line 78-82 to:

```yaml
- name: Perform CodeQL Analysis
  uses: github/codeql-action/analyze@v4
  with:
    category: "/language:${{ matrix.language }}"
    # Upload results to GitHub Security tab
    # Default setup has been disabled in repository settings
    upload: true
```

Remove these lines (they're now unnecessary):
- `upload-database: false`
- `wait-for-processing: false`

### Step 3: Verify Configuration
1. Commit and push the workflow changes
2. Trigger a workflow run (push to main or create PR)
3. Check **Security** tab → **Code scanning** for results
4. Verify both C# and JavaScript/TypeScript results appear

## Alternative Solution: Use Default Setup Only

If you prefer GitHub's managed solution:

### Step 1: Delete Custom Workflow
```bash
git rm .github/workflows/codeql.yml
git rm .github/codeql/codeql-config.yml  # optional, if not needed
```

### Step 2: Ensure Default Setup is Enabled
1. Repository **Settings** → **Code security and analysis**
2. Ensure **CodeQL analysis** is set to **Default setup**

**Trade-offs:**
- ✅ Simpler configuration
- ✅ GitHub manages updates
- ❌ Less control over paths analyzed
- ❌ Cannot customize queries easily
- ❌ Configuration not in version control

## Current Recommendation

**Keep the custom workflow** because:
1. It already has proven security analysis (see `CODEQL-ANALYSIS.md`)
2. Path filtering reduces noise from docs and vendored code
3. Configuration is version-controlled and reviewable
4. Team has already invested in customizing it

The only change needed is to **disable GitHub default setup** and **enable uploads** in the custom workflow.

## Migration Checklist

- [ ] 1. Disable GitHub default setup in repository settings
- [ ] 2. Update `codeql.yml` to set `upload: true`
- [ ] 3. Remove `upload-database: false` and `wait-for-processing: false`
- [ ] 4. Update comments in workflow file
- [ ] 5. Commit and push changes
- [ ] 6. Verify workflow runs successfully
- [ ] 7. Check Security tab for CodeQL results
- [ ] 8. Update `CODEQL-ANALYSIS.md` if needed

## References

- [GitHub CodeQL Documentation](https://docs.github.com/en/code-security/code-scanning/automatically-scanning-your-code-for-vulnerabilities-and-errors/about-code-scanning-with-codeql)
- [Configuring Code Scanning](https://docs.github.com/en/code-security/code-scanning/automatically-scanning-your-code-for-vulnerabilities-and-errors/configuring-code-scanning)
- [CodeQL Action Documentation](https://github.com/github/codeql-action)
- Project: [CODEQL-ANALYSIS.md](../CODEQL-ANALYSIS.md) - Security audit results
- Project: [SECURITY-FIXES.md](../SECURITY-FIXES.md) - Security improvements

## Questions?

If you have questions about this configuration, please open an issue or contact the maintainers.
