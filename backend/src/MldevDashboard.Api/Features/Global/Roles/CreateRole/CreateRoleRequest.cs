namespace MldevDashboard.Api.Features.Global.Roles.CreateRole;

public sealed record CreateRoleRequest(
    int SystemId,
    string Name,
    string DisplayName,
    string[] Permissions);
