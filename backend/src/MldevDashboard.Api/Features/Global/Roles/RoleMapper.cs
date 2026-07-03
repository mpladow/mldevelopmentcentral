using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Global.Roles;

internal static class RoleMapper
{
    public static RoleResponse ToResponse(ApplicationRole role)
    {
        return new RoleResponse(
            role.Id,
            role.SystemDefinitionId,
            role.SystemDefinition.SystemKey,
            role.SystemDefinition.Label,
            role.Name,
            role.DisplayName,
            role.IsProtected,
            role.RolePermissions
                .Select(rolePermission => rolePermission.ApplicationPermission.PermissionKey)
                .OrderBy(permission => permission)
                .ToArray());
    }
}
