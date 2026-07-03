using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Api.Features.Global.Systems.DeleteSystem;

public sealed class DeleteSystemHandler(MldevDashboardDbContext dbContext)
{
    public async Task<ApplicationResult<bool>> HandleAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var system = await dbContext.Systems
            .SingleOrDefaultAsync(existingSystem => existingSystem.Id == id, cancellationToken);
        if (system is null)
        {
            return ApplicationResult<bool>.NotFound("System not found.");
        }

        if (system.SystemKey == "global")
        {
            return ApplicationResult<bool>.BadRequest("The Global system cannot be deleted.");
        }

        var roleIds = await dbContext.ApplicationRoles
            .Where(role => role.SystemDefinitionId == id)
            .Select(role => role.Id)
            .ToArrayAsync(cancellationToken);
        var permissionIds = await dbContext.ApplicationPermissions
            .Where(permission => permission.SystemDefinitionId == id)
            .Select(permission => permission.Id)
            .ToArrayAsync(cancellationToken);

        var rolePermissions = await dbContext.ApplicationRolePermissions
            .Where(rolePermission =>
                roleIds.Contains(rolePermission.ApplicationRoleId) ||
                permissionIds.Contains(rolePermission.ApplicationPermissionId))
            .ToArrayAsync(cancellationToken);
        var accountRoles = await dbContext.SystemAccountRoles
            .Where(accountRole => accountRole.SystemDefinitionId == id)
            .ToArrayAsync(cancellationToken);
        var legacyAccountAccesses = await dbContext.SystemAccountAccesses
            .Where(accountAccess => accountAccess.SystemDefinitionId == id)
            .ToArrayAsync(cancellationToken);
        var roles = await dbContext.ApplicationRoles
            .Where(role => role.SystemDefinitionId == id)
            .ToArrayAsync(cancellationToken);
        var permissions = await dbContext.ApplicationPermissions
            .Where(permission => permission.SystemDefinitionId == id)
            .ToArrayAsync(cancellationToken);
        var theme = await dbContext.SystemThemeSettings
            .SingleOrDefaultAsync(themeSettings => themeSettings.SystemDefinitionId == id, cancellationToken);

        dbContext.ApplicationRolePermissions.RemoveRange(rolePermissions);
        dbContext.SystemAccountRoles.RemoveRange(accountRoles);
        dbContext.SystemAccountAccesses.RemoveRange(legacyAccountAccesses);
        if (theme is not null)
        {
            dbContext.SystemThemeSettings.Remove(theme);
        }
        dbContext.ApplicationRoles.RemoveRange(roles);
        dbContext.ApplicationPermissions.RemoveRange(permissions);
        dbContext.Systems.Remove(system);

        await dbContext.SaveChangesAsync(cancellationToken);
        return ApplicationResult<bool>.Success(true);
    }
}
