# Environment Setup

## Development Database

Use a full local SQL Server instance with the development connection string in:

```text
backend/src/MldevDashboard.Api/appsettings.Development.json
```

Current development database name:

```text
local-mldev-db
```

Apply migrations with:

```powershell
cd backend
dotnet ef database update --project src/MldevDashboard.Infrastructure/MldevDashboard.Infrastructure.csproj --startup-project src/MldevDashboard.Api/MldevDashboard.Api.csproj --context MldevDashboardDbContext
```

## Production Database

Production should use Azure SQL. Do not store production connection strings in source control.

Recommended production configuration locations:

- Azure App Service configuration
- Azure Key Vault

## Authentication Settings

Authentication placeholders live under:

```json
"Authentication": {
  "Jwt": {},
  "Microsoft": {}
}
```

JWT signing keys, Microsoft client IDs, tenant IDs, and allowed email lists should be provided through environment-specific secure configuration.

## Frontend

The React app is configured for Vite on:

```text
http://localhost:5173
```

The backend development CORS policy already allows this origin.
