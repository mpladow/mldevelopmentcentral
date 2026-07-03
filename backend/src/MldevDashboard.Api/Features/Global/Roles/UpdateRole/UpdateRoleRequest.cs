namespace MldevDashboard.Api.Features.Global.Roles.UpdateRole;

public sealed record UpdateRoleRequest(
    string DisplayName,
    string[] Permissions);
