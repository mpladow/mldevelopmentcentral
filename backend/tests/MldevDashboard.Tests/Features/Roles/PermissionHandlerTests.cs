using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Global.Roles.CreatePermission;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Tests.Features.Roles;

public sealed class PermissionHandlerTests
{
    [Fact]
    public async Task CreatePermissionHandler_CreatesSystemScopedPermission()
    {
        await using var dbContext = CreateDbContext();
        var system = new SystemDefinition { SystemKey = "warmaster", Label = "Warmaster" };
        dbContext.Systems.Add(system);
        await dbContext.SaveChangesAsync();
        var handler = new CreatePermissionHandler(dbContext);

        var result = await handler.HandleAsync(
            new CreatePermissionRequest(system.Id, "warmaster.menu.reports", "Reports menu", "Menus"),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(system.Id, result.Value.SystemId);
        Assert.Equal("warmaster.menu.reports", result.Value.PermissionKey);
        Assert.True(await dbContext.ApplicationPermissions.AnyAsync(permission =>
            permission.PermissionKey == "warmaster.menu.reports" &&
            permission.SystemDefinitionId == system.Id));
    }

    [Fact]
    public async Task CreatePermissionHandler_ReturnsConflictForDuplicatePermissionKey()
    {
        await using var dbContext = CreateDbContext();
        var system = new SystemDefinition { SystemKey = "warmaster", Label = "Warmaster" };
        dbContext.Systems.Add(system);
        dbContext.ApplicationPermissions.Add(new ApplicationPermission
        {
            SystemDefinition = system,
            PermissionKey = "warmaster.menu.reports",
            DisplayName = "Reports menu",
            Category = "Menus"
        });
        await dbContext.SaveChangesAsync();
        var handler = new CreatePermissionHandler(dbContext);

        var result = await handler.HandleAsync(
            new CreatePermissionRequest(system.Id, "warmaster.menu.reports", "Reports menu", "Menus"),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Conflict, result.Status);
        Assert.Equal("A permission already exists with this key.", result.Message);
    }

    private static MldevDashboardDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MldevDashboardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MldevDashboardDbContext(options);
    }
}
