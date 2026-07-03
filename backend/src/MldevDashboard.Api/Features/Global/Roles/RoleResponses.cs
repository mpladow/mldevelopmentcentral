namespace MldevDashboard.Api.Features.Global.Roles;

public sealed record RoleResponse(
    int Id,
    int SystemId,
    string SystemKey,
    string SystemLabel,
    string Name,
    string DisplayName,
    bool IsProtected,
    string[] Permissions);

public sealed record PermissionCatalogResponse(
    int Id,
    int SystemId,
    string SystemKey,
    string SystemLabel,
    string PermissionKey,
    string DisplayName,
    string Category);
