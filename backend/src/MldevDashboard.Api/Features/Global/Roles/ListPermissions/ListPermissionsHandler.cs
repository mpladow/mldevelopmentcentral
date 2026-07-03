using Microsoft.EntityFrameworkCore;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Api.Features.Global.Roles.ListPermissions;

public sealed class ListPermissionsHandler(MldevDashboardDbContext dbContext)
{
    public async Task<PermissionCatalogResponse[]> HandleAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ApplicationPermissions
            .AsNoTracking()
            .OrderBy(permission => permission.SystemDefinition.SortOrder)
            .ThenBy(permission => permission.Category)
            .ThenBy(permission => permission.PermissionKey)
            .Select(permission => new PermissionCatalogResponse(
                permission.Id,
                permission.SystemDefinitionId,
                permission.SystemDefinition.SystemKey,
                permission.SystemDefinition.Label,
                permission.PermissionKey,
                permission.DisplayName,
                permission.Category))
            .ToArrayAsync(cancellationToken);
    }
}
