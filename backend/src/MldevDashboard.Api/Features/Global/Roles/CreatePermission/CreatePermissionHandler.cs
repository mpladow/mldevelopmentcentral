using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Global.Roles.CreatePermission;

public sealed class CreatePermissionHandler(MldevDashboardDbContext dbContext)
{
    public async Task<ApplicationResult<PermissionCatalogResponse>> HandleAsync(
        CreatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionKey = request.PermissionKey.Trim();
        var displayName = request.DisplayName.Trim();
        var category = request.Category.Trim();

        var system = await dbContext.Systems
            .AsNoTracking()
            .SingleOrDefaultAsync(existingSystem => existingSystem.Id == request.SystemId, cancellationToken);
        if (system is null)
        {
            return ApplicationResult<PermissionCatalogResponse>.BadRequest("System not found.");
        }

        var keyExists = await dbContext.ApplicationPermissions.AnyAsync(
            permission => permission.PermissionKey == permissionKey,
            cancellationToken);
        if (keyExists)
        {
            return ApplicationResult<PermissionCatalogResponse>.Conflict("A permission already exists with this key.");
        }

        var permission = new ApplicationPermission
        {
            SystemDefinitionId = system.Id,
            PermissionKey = permissionKey,
            DisplayName = displayName,
            Category = category
        };

        dbContext.ApplicationPermissions.Add(permission);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApplicationResult<PermissionCatalogResponse>.Created(new PermissionCatalogResponse(
            permission.Id,
            system.Id,
            system.SystemKey,
            system.Label,
            permission.PermissionKey,
            permission.DisplayName,
            permission.Category));
    }
}
