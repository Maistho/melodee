# Melodee Design Documentation

This directory contains internal design documentation, implementation notes, testing guides, and other technical documents that are **NOT** part of the public Jekyll documentation site.

## Directory Structure

| Directory | Purpose |
|-----------|---------|
| `adr/` | Architecture Decision Records - documenting significant architectural decisions |
| `backlog/` | Feature backlog and planning documents |
| `charts/` | Chart data files (JSON) for importing music charts |
| `docs/` | Implementation documentation, session summaries, fix documentation |
| `requirements/` | Feature requirements and specifications |
| `runbooks/` | Operational runbooks for debugging and maintenance |
| `testing/` | Manual test guides, test coverage reports, testing documentation |

## Important Note for AI Agents

**DO NOT create documentation files in the `/docs` directory.**

The `/docs` directory is exclusively for the Jekyll-based GitHub Pages documentation site. Files placed there will be:
- Published to the public documentation website
- Subject to Jekyll processing and formatting requirements
- Visible to end users

Instead, place internal documentation in the appropriate subdirectory here in `/design`:

| Document Type | Location |
|---------------|----------|
| Implementation notes | `design/docs/` |
| Session summaries | `design/docs/` |
| Fix documentation | `design/docs/` |
| Requirements specs | `design/requirements/` |
| Testing guides | `design/testing/` |
| Test reports | `design/testing/` |
| Runbooks | `design/runbooks/` |
| ADRs | `design/adr/` |

## Public Documentation

To add or modify **public-facing documentation** (user guides, API docs, etc.), edit files in:
- `/docs/pages/` - Main documentation pages
- `/docs/_data/toc.yml` - Table of contents navigation

Public documentation uses Jekyll with the following front matter format:

```yaml
---
title: Page Title
description: Brief description
tags:
  - relevant-tag
---
```
