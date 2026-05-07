# diet-tracker-api

ASP.NET Core Web API for the Diet Tracker application. This service provides REST endpoints for managing diet-tracking data, uses Entity Framework Core with PostgreSQL, and secures access with JWT bearer authentication.

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- PostgreSQL via Npgsql
- JWT bearer authentication
- Swagger / OpenAPI
- Docker and docker-compose
- PowerShell migration scripts

## Repository Structure

- `Authorization/` - authorization handlers and requirements
- `BusinessLayer/` - application/business logic
- `Controllers/` - API controllers and HTTP endpoints
- `DataLayer/` - EF Core data access and models
- `Extensions/` - application extension helpers
- `Filters/` - MVC/API filters
- `Migrations/` - Entity Framework database migrations
- `Services/` - supporting services such as token generation
- `scripts/` - migration and data import/export utilities
- `Program.cs` - application startup and dependency registration
- `docker-compose.template.yaml` - local/container deployment template
- `POSTGRESQL_MIGRATION.md` - PostgreSQL migration guide

## Features

- REST API built with ASP.NET Core controllers
- JWT authentication and authorization policies
- PostgreSQL database connectivity with automatic EF Core migrations at startup
- Health check endpoint at `/health`
- Swagger UI for API exploration
- Docker-based local deployment template
- Data migration tooling for moving from MS SQL Server to PostgreSQL

## Configuration

The API expects configuration from environment variables.

### Required JWT settings

- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:SecretKey`

### Required database settings

- `DB_HOST`
- `DB_PORT`
- `DB_NAME`
- `DB_USERNAME`
- `DB_PASSWORD`

## Running Locally

### Using .NET CLI

```bash
# restore dependencies
 dotnet restore

# run the API
 dotnet run
```

By default, Swagger is enabled and the app also exposes a health endpoint at `/health`.

### Using Docker Compose

1. Copy `docker-compose.template.yaml` to your own compose file.
2. Replace placeholder values for PostgreSQL and JWT configuration.
3. Start the stack:

```bash
docker compose up -d
```

The template includes:

- `postgres`
- `diet-tracker-api`
- `diet-tracker-ui`

## Authentication

The application is configured to use JWT bearer authentication. Tokens are validated using issuer, audience, signing key, and lifetime checks. Authorization policies are registered for permission-based scopes such as:

- `write:fuelings`
- `write:plans`
- `write:lean-and-greens`
- `write:user`
- `read:user`
- `read:user:fuelings`
- `read:user:lean-and-green`
- `admin:users`

## Database Notes

- The application uses PostgreSQL as its active database provider.
- EF Core migrations are applied automatically during startup.
- See `POSTGRESQL_MIGRATION.md` for migration and data import/export guidance.

## Revision History

Recent notable changes based on commit history:

- **2026-05-07** - Updated handling for user name data (`handle user name`).
- **2026-05-05** - Implemented native JWT authentication with refresh tokens, added refresh token and credential models, updated configuration, and removed Auth0 management dependencies.
- **2025-11-27** - Refactored controllers to remove unused logger dependencies and improved database configuration error handling.
- **2025-11-27** - Completed several PostgreSQL migration improvements, including timestamp compatibility updates, docker compose improvements, schema check/export-import scripts, and merge of the database migration work.

For additional history, see the commit list on GitHub:
https://github.com/clinta74/diet-tracker-api/commits/main

## Related Documentation

- `POSTGRESQL_MIGRATION.md` - migration steps and troubleshooting

## License

No license file is currently defined in this repository.
