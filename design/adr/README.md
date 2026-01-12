## ADRs (Architecture Decision Records)

This directory contains Melodee’s Architecture Decision Records (ADRs): short, durable documents that capture **why** a significant architectural or product decision was made.

### When to write an ADR

Write an ADR when a change is:

- A meaningful product/architecture decision (not just an implementation detail)
- Hard to reverse or likely to be debated later
- Expected to influence multiple parts of the system or future roadmap

### ADR lifecycle

- **Accepted**: the decision is in effect.
- **Superseded**: a newer ADR replaces this one; do not delete history.
- **Deprecated**: no longer recommended, but not necessarily replaced.

If a decision changes, prefer creating a **new ADR** and marking the old one **Superseded**.

### File naming

Use one ADR per file.

**Casing preference:** Prefer readable kebab-case for filenames and avoid ALL-CAPS filenames (harder to scan in listings).

- `ADR-0001-short-title.md`

### ADR template

Copy/paste and fill out:

```markdown
## ADR-000X: <short title>

- Date: 2026-01-09T04:13:52.861Z
- Status: Proposed | Accepted | Superseded | Deprecated

### Context

What problem are we solving? What constraints exist? What alternatives were considered?

### Decision

What did we decide to do?

### Rationale

Why is this the best choice right now?

### Consequences

What trade-offs does this introduce? What follow-up work is required?

### References

- Links to requirements docs, PRs, issues, or related ADRs
```
