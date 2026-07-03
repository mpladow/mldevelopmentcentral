using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Global.Roles.CreateRole;

public sealed class CreateRoleHandler(MldevDashboardDbContext dbContext)
{
    public async Task<ApplicationResult<RoleResponse>> HandleAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var displayName = request.DisplayName.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(displayName))
        {
            return ApplicationResult<RoleResponse>.BadRequest("Role name and display name are required.");
        }

        if (await dbContext.ApplicationRoles.AnyAsync(role => role.Name == name, cancellationToken))
        {
            return ApplicationResult<RoleResponse>.Conflict("A role already exists with this name.");
        }

        var system = await dbContext.Systems.SingleOrDefaultAsync(
            system => system.Id == request.SystemId,
            cancellationToken);
        if (system is null)
        {
            return ApplicationResult<RoleResponse>.BadRequest("System not found.");
        }

        var permissions = await GetValidPermissionsAsync(request.Permissions, request.SystemId, cancellationToken);
        var invalidPermissions = GetInvalidPermissions(request.Permissions, permissions);
        if (invalidPermissions.Length > 0)
        {
            return ApplicationResult<RoleResponse>.BadRequest(
                $"Unknown permission(s): {string.Join(", ", invalidPermissions)}");
        }

        var sortOrder = await dbContext.ApplicationRoles
            .Where(role => role.SystemDefinitionId == request.SystemId)
            .Select(role => (int?)role.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var role = new ApplicationRole
        {
            SystemDefinitionId = request.SystemId,
            Name = name,
            DisplayName = displayName,
            SortOrder = sortOrder + 1,
            RolePermissions = permissions
                .Select(permission => new ApplicationRolePermission { ApplicationPermissionId = permission.Id })
                .ToList()
        };

        dbContext.ApplicationRoles.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(role).Reference(nextRole => nextRole.SystemDefinition).LoadAsync(cancellationToken);
        await dbContext.Entry(role).Collection(nextRole => nextRole.RolePermissions).Query()
            .Include(rolePermission => rolePermission.ApplicationPermission)
            .LoadAsync(cancellationToken);

        return ApplicationResult<RoleResponse>.Created(RoleMapper.ToResponse(role));
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
