using Microsoft.EntityFrameworkCore;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Identity;

public sealed class AppAuthorizationService(MldevDashboardDbContext dbContext)
{
    public async Task<AppAccess> GetAccessAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var roles = await dbContext.SystemAccountRoles
            .Where(accountRole => accountRole.AccountId == accountId)
            .Select(accountRole => accountRole.ApplicationRole.Name)
            .Distinct()
            .OrderBy(role => role)
            .ToArrayAsync(cancellationToken);

        var permissions = await dbContext.SystemAccountRoles
            .Where(accountRole => accountRole.AccountId == accountId)
            .SelectMany(accountRole => accountRole.ApplicationRole.RolePermissions)
            .Select(rolePermission => rolePermission.ApplicationPermission.PermissionKey)
            .Distinct()
            .OrderBy(permission => permission)
            .ToArrayAsync(cancellationToken);

        return new AppAccess(roles, permissions);
    }

    public async Task<ApplicationRole[]> GetRolesByNameAsync(
        string[] roleNames,
        CancellationToken cancellationToken)
    {
        var normalizedRoleNames = roleNames
            .Select(role => role.Trim())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return await dbContext.ApplicationRoles
            .Where(role => normalizedRoleNames.Contains(role.Name))
            .OrderBy(role => role.SystemDefinition.SortOrder)
            .ThenBy(role => role.SortOrder)
            .ToArrayAsync(cancellationToken);
    }
}

public sealed record AppAccess(string[] Roles, string[] Permissions);
