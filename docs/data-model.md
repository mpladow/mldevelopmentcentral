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
