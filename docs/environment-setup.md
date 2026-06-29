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

For local development, use .NET user secrets for sensitive values:

```powershell
cd backend
dotnet user-secrets set "SeedAdmin:Password" "<local-admin-password>" --project src/MldevDashboard.Api/MldevDashboard.Api.csproj
dotnet user-secrets set "Authentication:Jwt:SigningKey" "<local-jwt-signing-key>" --project src/MldevDashboard.Api/MldevDashboard.Api.csproj
```

The initial admin email is configured as:

```text
ml.development.2022@gmail.com
```

The initial roles are:

```text
Admin
User
Viewer
```

The current authentication endpoints are:

```text
POST /api/auth/login
GET  /api/auth/me
GET  /api/accounts
POST /api/accounts
GET  /api/roles
```

## Frontend

The React app is configured for Vite on:

```text
http://localhost:5173
```

The backend development CORS policy already allows this origin.
