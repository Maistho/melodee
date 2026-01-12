# Contributing to Melodee

First off, thank you for considering contributing to Melodee! It's people like you that make Melodee such a great tool for the self-hosted music community.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Features](#suggesting-features)
  - [Contributing Code](#contributing-code)
  - [Contributing Translations](#contributing-translations)
  - [Improving Documentation](#improving-documentation)
- [Development Setup](#development-setup)
- [Pull Request Process](#pull-request-process)
- [Style Guidelines](#style-guidelines)
- [Community](#community)

## Code of Conduct

This project and everyone participating in it is governed by the [Melodee Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior via [Discord](https://discord.gg/bfMnEUrvbp) or by opening an issue.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the [existing issues](https://github.com/melodee-project/melodee/issues) to avoid duplicates.

When creating a bug report, please include:

- **Clear title** describing the issue
- **Steps to reproduce** the behavior
- **Expected behavior** vs **actual behavior**
- **Screenshots** if applicable
- **Environment details**:
  - Melodee version (check `/admin/doctor`)
  - Deployment method (Docker/Podman, bare metal)
  - Browser and version (for UI issues)
  - Client app and version (for API issues)

Use the bug report template when available.

### Suggesting Features

Feature suggestions are welcome! Please:

1. Check [existing issues](https://github.com/melodee-project/melodee/issues) and [discussions](https://github.com/melodee-project/melodee/discussions) first
2. Open a new discussion in the "Ideas" category
3. Describe the feature and its use case
4. Be open to feedback and iteration

### Contributing Code

#### Good First Issues

Looking for a place to start? Check out issues labeled [`good first issue`](https://github.com/melodee-project/melodee/labels/good%20first%20issue) or [`help wanted`](https://github.com/melodee-project/melodee/labels/help%20wanted).

#### Before You Start

1. **Open an issue first** for significant changes to discuss the approach
2. **Check existing PRs** to avoid duplicate work
3. **Fork the repository** and create your branch from `main`

### Contributing Translations

Melodee supports 10 languages and we welcome translation contributions! This is a great way to contribute without writing code.

**📖 See [CONTRIBUTING_TRANSLATIONS.md](CONTRIBUTING_TRANSLATIONS.md) for the complete translation guide.**

#### Quick Start

1. **Find your language file**: `src/Melodee.Blazor/Resources/SharedResources.<code>.resx`
2. **Search for** `[NEEDS TRANSLATION]` entries
3. **Replace** the placeholder with your native translation
4. **Submit a pull request**

**Example:**
```xml
<!-- Before -->
<data name="Actions.Save" xml:space="preserve">
  <value>[NEEDS TRANSLATION] Save</value>
</data>

<!-- After -->
<data name="Actions.Save" xml:space="preserve">
  <value>Guardar</value>
</data>
```

**Supported languages**: English, German, Spanish, French, Italian, Japanese, Portuguese (Brazil), Russian, Chinese (Simplified), Arabic

### Improving Documentation

Documentation improvements are always welcome:

- Fix typos or clarify existing docs
- Add examples or use cases
- Improve the README
- Write guides for common tasks

## Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 17](https://www.postgresql.org/) (or use Docker)
- [Node.js](https://nodejs.org/) (for frontend tooling, optional)
- Git

### Quick Start

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/melodee.git
cd melodee

# Create a branch for your changes
git checkout -b feature/your-feature-name

# Restore dependencies
dotnet restore

# Run the application
dotnet run --project src/Melodee.Blazor

# Run tests
dotnet test
```

### Using Docker for Development

```bash
# Start PostgreSQL
docker run -d --name melodee-db \
  -e POSTGRES_USER=melodeeuser \
  -e POSTGRES_PASSWORD=devpassword \
  -e POSTGRES_DB=melodeedb \
  -p 5432:5432 \
  postgres:17

# Update connection string in appsettings.Development.json
# Then run the app
dotnet run --project src/Melodee.Blazor
```

### Project Structure

```
melodee/
├── src/
│   ├── Melodee.Blazor/      # Web UI and API server
│   ├── Melodee.Cli/         # Command-line interface
│   └── Melodee.Common/      # Shared libraries
├── tests/
│   ├── Melodee.Tests/       # Unit tests
│   └── Melodee.Tests.Common/ # Common test utilities
├── docs/                    # Documentation
└── scripts/                 # Build and utility scripts
```

## Pull Request Process

### 1. Before Submitting

- [ ] Code compiles without errors: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] New code has appropriate test coverage
- [ ] Code follows the [style guidelines](#style-guidelines)
- [ ] Documentation is updated if needed
- [ ] Commit messages are clear and descriptive

### 2. Submitting

1. Push your branch to your fork
2. Open a pull request against `main`
3. Fill out the PR template completely
4. Link any related issues

### 3. Review Process

- A maintainer will review your PR
- Address any requested changes
- Once approved, a maintainer will merge your PR

### Commit Message Guidelines

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Code style (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(api): add endpoint for playlist export
fix(ui): correct album art aspect ratio on mobile
docs: update installation instructions
```

## Style Guidelines

### C# Code Style

- Follow the [.NET coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use the project's `.editorconfig` settings
- Run `dotnet format` before committing
- Use meaningful names for variables, methods, and classes
- Add XML documentation for public APIs

### Blazor/Razor Guidelines

- Use Radzen components consistently
- Follow the localization patterns (use `L("Key")` for all user-facing text)
- Keep components focused and reasonably sized
- Use `@inherits MelodeeComponentBase` for pages needing localization

### Testing

- Write unit tests for new functionality
- Follow the existing test patterns
- Use meaningful test names that describe the scenario
- Aim for good coverage of edge cases

## Community

- **Discord**: [Join our community](https://discord.gg/bfMnEUrvbp)
- **Discussions**: [GitHub Discussions](https://github.com/melodee-project/melodee/discussions)
- **Issues**: [GitHub Issues](https://github.com/melodee-project/melodee/issues)

## Recognition

Contributors are recognized in:
- The project's release notes
- The GitHub contributors page

Thank you for contributing to Melodee! 🎵
