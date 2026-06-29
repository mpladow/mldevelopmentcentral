# ML Dev Dashboard

Internal CRM/admin dashboard built with ASP.NET Core, Entity Framework Core, Azure SQL, and React TypeScript.

## Structure

```text
backend/
  MldevDashboard.sln
  src/
    MldevDashboard.Api/
    MldevDashboard.Application/
    MldevDashboard.Domain/
    MldevDashboard.Infrastructure/
frontend/
  src/
docs/
```

## Current Backend Scope

- ASP.NET Core API shell on .NET 8.
- Layered backend projects.
- EF Core SQL Server configured.
- Initial `Systems` table model and migration.
- No CRUD controllers yet.

## Common Commands

```powershell
cd backend
dotnet build
dotnet ef database update --project src/MldevDashboard.Infrastructure/MldevDashboard.Infrastructure.csproj --startup-project src/MldevDashboard.Api/MldevDashboard.Api.csproj --context MldevDashboardDbContext
```

```powershell
cd frontend
npm install
npm run dev
```
