# Agent Guide

## Project Overview

ML Dev Dashboard is an internal CRM/admin dashboard for managing shared reference data and files across multiple projects.

The repository is split into:

- `frontend/` - React TypeScript web app.
- `backend/` - ASP.NET Core API solution.
- `docs/` - Project setup and data model notes.

## Current Stack

### Frontend

- React 19 with TypeScript.
- Vite 7 for local development and production builds.
- Vitest with jsdom for frontend tests.
- Testing Library for React component tests.
- ESLint 9 with TypeScript and React hooks rules.
- lucide-react for UI icons.
- API base URL configured with `VITE_API_BASE_URL`, defaulting locally to `http://localhost:5058`.

### Backend

- .NET 8 ASP.NET Core API.
- Entity Framework Core with SQL Server.
- Layered solution structure:
  - `MldevDashboard.Api`
  - `MldevDashboard.Application`
  - `MldevDashboard.Domain`
  - `MldevDashboard.Infrastructure`
- ASP.NET Core Identity and JWT authentication.
- EF Core migrations in the Infrastructure project.

## Common Commands

Run commands from the repository root unless a `cd` command is shown.

### Frontend

```powershell
cd frontend
npm install
npm run dev
```

```powershell
cd frontend
npm run build
npm run lint
npm run test
```

```powershell
cd frontend
npm run preview
```

### Backend

```powershell
cd backend
dotnet restore
dotnet build
dotnet test
```

```powershell
cd backend
dotnet run --project src/MldevDashboard.Api/MldevDashboard.Api.csproj
```

```powershell
cd backend
dotnet ef database update --project src/MldevDashboard.Infrastructure/MldevDashboard.Infrastructure.csproj --startup-project src/MldevDashboard.Api/MldevDashboard.Api.csproj --context MldevDashboardDbContext
```

## Frontend Development Guidelines

- Keep components and files relatively small.
- Prefer extracting focused UI pieces into separate components when a file starts mixing unrelated responsibilities.
- Pull reusable logic into hooks or utility modules instead of repeating it inside views.
- Keep API request logic isolated from presentational components where practical.
- Use existing styling patterns in `frontend/src/styles.css` before introducing new visual conventions.
- Use lucide-react icons for action buttons and navigation affordances when an appropriate icon exists.
- Keep views ergonomic and utilitarian; this is an internal admin dashboard, so prioritize clear scanning, predictable controls, and compact workflows.

## Infrastructure And Modularity Guidelines

- Preserve the backend layer boundaries: API endpoints should delegate business behavior to Application services, Application should depend on Domain abstractions, and Infrastructure should contain EF Core and external-service implementations.
- Keep cross-cutting behavior in small, named services or extension methods rather than growing large endpoint or component files.
- Prefer focused request/response models, service methods, and components over broad objects that serve several workflows at once.
- When adding new features, consider whether the feature belongs in its own folder or module so future dashboard areas can evolve independently.
- Avoid introducing shared abstractions until there is real duplication or a clear boundary to protect.

## Verification Expectations

- For frontend changes, run `npm run lint`, `npm run test`, and `npm run build` when relevant.
- For backend changes, run `dotnet test` and `dotnet build` when relevant.
- If a command cannot be run locally, note why and describe the remaining risk.
