---
description: 'YAML file conventions for configuration, Docker Compose, and CI/CD'
applyTo: '**/*.yml, **/*.yaml'
---

# YAML Configuration

## General Guidelines

- Use 2-space indentation consistently
- Use lowercase keys with hyphens for multi-word names
- Quote strings that could be interpreted as other types (yes/no, on/off, numbers)
- Use explicit types when ambiguity exists

## Structure

- Keep files focused on single purpose
- Use comments to explain non-obvious configurations
- Group related settings together
- Order keys logically or alphabetically within sections

```yaml
# Database configuration
database:
  host: localhost
  port: 5432
  name: melodee
  # Connection pool settings
  pool:
    min-size: 5
    max-size: 20
```

## Docker Compose Specific

- Define services in logical order (dependencies first)
- Use named volumes for data persistence
- Define explicit networks for service communication
- Include health checks for critical services
- Use environment variable substitution for configurability

```yaml
services:
  database:
    image: postgres:16
    volumes:
      - db-data:/var/lib/postgresql/data
    environment:
      POSTGRES_DB: ${DB_NAME:-melodee}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  app:
    build: .
    depends_on:
      database:
        condition: service_healthy
    ports:
      - "${APP_PORT:-8080}:8080"

volumes:
  db-data:
```

## CI/CD Configuration

- Use descriptive job and step names
- Cache dependencies to speed up builds
- Use matrix builds for multi-platform testing
- Store secrets in CI/CD platform, not in files
- Use conditional execution for optional steps

## Jekyll Configuration

- Use appropriate data types for settings
- Keep sensitive data out of `_config.yml`
- Use collections for structured content
- Define defaults to reduce repetition in front matter

## Validation

- Validate YAML syntax before committing
- Use schema validation when available (JSON Schema for compose files)
- Test configuration changes in non-production environments first

## Security

- Never commit secrets or credentials in YAML files
- Use environment variable references for sensitive values
- Review changes to CI/CD configuration carefully
- Restrict permissions in CI/CD job definitions
