# Within.Seed

Manual demo data seeding tool for local, test, or controlled demo databases.

The API no longer inserts seed data at startup. Run this project only when you intentionally want the demo users, providers, events, communities, posts, reactions, registrations, and wellbeing check-ins inserted or updated.

## Commands

From `WithinAPI`:

```powershell
dotnet run --project Within.Seed\Within.Seed.csproj -- --apply
```

Apply migrations first, then seed:

```powershell
dotnet run --project Within.Seed\Within.Seed.csproj -- --apply --migrate
```

Without `--apply`, the tool prints usage and exits without touching the database.

## Configuration

The tool reads the same `ConnectionStrings:WithinPostgres` setting as the API:

- `appsettings.json`
- `appsettings.{DOTNET_ENVIRONMENT}.json` or `appsettings.{ASPNETCORE_ENVIRONMENT}.json`
- environment variables
- command-line configuration

Use a production connection string only when you are deliberately seeding production-like data.
