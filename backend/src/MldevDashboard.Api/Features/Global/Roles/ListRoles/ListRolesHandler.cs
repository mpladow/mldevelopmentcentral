using Microsoft.EntityFrameworkCore;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Api.Features.Global.Roles.ListRoles;

public sealed class ListRolesHandler(MldevDashboardDbContext dbContext)
{
    public async Task<RoleResponse[]> HandleAsync(CancellationToken cancellationToken)
    {
        var roles = await dbContext.ApplicationRoles
            .AsNoTracking()
            .Include(role => role.SystemDefinition)
            .Include(role => role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.ApplicationPermission)
            .OrderBy(role => role.SystemDefinition.SortOrder)
            .ThenBy(role => role.SortOrder)
            .ThenBy(role => role.Name)
            .ToArrayAsync(cancellationToken);

        return roles.Select(RoleMapper.ToResponse).ToArray();
    }
}
