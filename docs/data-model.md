# Data Model

## Systems

The first table is mapped from `SystemDefinition` to `Systems`.

| Column | Type | Notes |
| --- | --- | --- |
| Id | int | Primary key, identity |
| SystemKey | nvarchar(64) | Required, unique |
| Label | nvarchar(128) | Required |
| SortOrder | int | Required |
| IsActive | bit | Required, defaults to true |

`SystemDefinition` is used as the C# entity name to avoid conflicts with the built-in `System` namespace.

## Identity

ASP.NET Core Identity is used for Global account and role management.

Initial roles:

| Role | Purpose |
| --- | --- |
| Admin | Can create accounts and manage Global access |
| User | Standard authenticated user |
| Viewer | Read-only user |

The initial admin account is seeded from configuration when both `SeedAdmin:Email` and `SeedAdmin:Password` are available. The password must come from user secrets or secure environment configuration, not committed settings files.
