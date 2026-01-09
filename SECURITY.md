## Security Policy

This document describes how to report security issues for Melodee.

### Supported versions

Melodee is primarily distributed from source and via containers.

- **Supported**: the current default branch (`main`) and the latest tagged release (if tags exist).
- **Unsupported**: older commits/tags and unofficial third-party builds.

If you are unsure whether your version is supported, still report the issue; we will advise on next steps.

### Reporting a vulnerability

Please **do not** open a public GitHub Issue for security vulnerabilities.

Preferred reporting channel:

1. Go to the repository **Security** tab.
2. Click **Report a vulnerability** (GitHub Security Advisories).
3. Provide the details requested below.

If the Security tab is not available in your fork, report the issue in the upstream repository.

### What to include

To help us triage quickly, please include:

- A clear description of the vulnerability and its impact.
- Affected component(s) and version/commit SHA.
- Steps to reproduce (proof-of-concept is helpful).
- Any known mitigations or configuration constraints.
- Whether you can reliably reproduce the issue.

If your report includes sensitive data, please redact it.

### What to expect

We aim to follow common coordinated disclosure practices:

- **Acknowledgement**: within 72 hours.
- **Triage**: we will assess severity, impact, and affected versions.
- **Fix development**: timelines vary by severity and complexity.
- **Disclosure**: we will coordinate with you on a reasonable disclosure date once a fix is available.

### Security advisories and updates

When a vulnerability is confirmed, we will generally:

- Publish a GitHub Security Advisory (CVE if appropriate).
- Document upgrade/mitigation guidance.
- Provide a patched tag/container image where possible.

### Scope

This policy covers security issues in:

- The Melodee server and its APIs.
- Official container images (if/when published).
- Repository-managed configuration and deployment artifacts.

Third-party dependencies (NuGet packages, base container images, etc.) should still be reported if they are exploitable through Melodee.

### Safe harbor

We support good-faith security research intended to improve the security of Melodee and its users.

- Do not access or modify data that does not belong to you.
- Do not perform testing that degrades availability for other users.
- Do not use social engineering, phishing, or physical attacks.

### Credits

If you would like to be credited for a report/fix, let us know in the advisory.
