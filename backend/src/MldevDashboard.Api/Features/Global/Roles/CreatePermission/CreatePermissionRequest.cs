namespace MldevDashboard.Api.Features.Global.Roles.CreatePermission;

public sealed record CreatePermissionRequest(
    int SystemId,
    string PermissionKey,
    string DisplayName,
    string Category);
