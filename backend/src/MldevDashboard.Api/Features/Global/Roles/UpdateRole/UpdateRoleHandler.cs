using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Global.Roles.UpdateRole;

public sealed class UpdateRoleHandler(MldevDashboardDbContext dbContext)
{
    public async Task<ApplicationResult<RoleResponse>> HandleAsync(
        int id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await dbContext.ApplicationRoles
            .Include(existingRole => existingRole.SystemDefinition)
            .Include(existingRole => existingRole.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.ApplicationPermission)
            .SingleOrDefaultAsync(existingRole => existingRole.Id == id, cancellationToken);
        if (role is null)
        {
            return ApplicationResult<RoleResponse>.NotFound("Role not found.");
        }

        var displayName = request.DisplayName.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return ApplicationResult<RoleResponse>.BadRequest("Display name is required.");
        }

        var permissions = await GetValidPermissionsAsync(
            request.Permissions,
            role.SystemDefinitionId,
            cancellationToken);
        var invalidPermissions = GetInvalidPermissions(request.Permissions, permissions);
        if (invalidPermissions.Length > 0)
        {
            return ApplicationResult<RoleResponse>.BadRequest(
                $"Unknown permission(s): {string.Join(", ", invalidPermissions)}");
        }

        if (role.Name == ApplicationRoleNames.GlobalAdmin)
        {
            var missingMinimumPermissions = AppPermissions.GlobalAdminMinimum
                .Except(permissions.Select(permission => permission.PermissionKey), StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (missingMinimumPermissions.Length > 0)
            {
                return ApplicationResult<RoleResponse>.BadRequest(
                    $"GlobalAdmin requires: {string.Join(", ", missingMinimumPermissions)}");
            }
        }

        role.DisplayName = displayName;
        role.RolePermissions.Clear();
        foreach (var permission in permissions)
        {
            role.RolePermissions.Add(new ApplicationRolePermission
            {
                ApplicationRoleId = role.Id,
                ApplicationPermissionId = permission.Id
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ApplicationResult<RoleResponse>.Success(RoleMapper.ToResponse(role));
    }

    private async Task<ApplicationPermission[]> GetValidPermissionsAsync(
        string[] requestedPermissions,
        int systemId,
        CancellationToken cancellationToken)
    {
        var permissionKeys = NormalizePermissions(requestedPermissions);
        return await dbContext.ApplicationPermissions
            .Where(permission =>
                permission.SystemDefinitionId == systemId &&
                permissionKeys.Contains(permission.PermissionKey))
            .ToArrayAsync(cancellationToken);
    }

    private static string[] GetInvalidPermissions(
        string[] requestedPermissions,
        ApplicationPermission[] permissions)
    {
        return NormalizePermissions(requestedPermissions)
            .Except(permissions.Select(permission => permission.PermissionKey), StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] NormalizePermissions(string[] requestedPermissions)
    {
        return requestedPermissions
            .Select(permission => permission.Trim())
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
