using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Identity;

public static class IdentitySeeder
{
    public static async Task SeedIdentityAsync(
        this IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<MldevDashboardDbContext>();

        var globalSystem = await EnsureSystemAsync(dbContext, "global", "Global", 0);
        await EnsurePermissionsAsync(dbContext, globalSystem, AppPermissions.Global);
        await EnsureRoleAsync(dbContext, globalSystem, ApplicationRoleNames.GlobalUser, "Global User", 1);
        await EnsureRoleAsync(dbContext, globalSystem, ApplicationRoleNames.GlobalViewer, "Global Viewer", 2);
        var globalAdminRole = await EnsureRoleAsync(
            dbContext,
            globalSystem,
            ApplicationRoleNames.GlobalAdmin,
            "Global Admin",
            0,
            isProtected: true);
        await EnsureRolePermissionsAsync(dbContext, globalAdminRole, AppPermissions.GlobalAdminMinimum);

        var warmasterSystem = await EnsureSystemAsync(dbContext, "warmaster", "Warmaster", 1);
        await EnsurePermissionsAsync(dbContext, warmasterSystem, AppPermissions.Warmaster);
        await EnsureRoleAsync(dbContext, warmasterSystem, ApplicationRoleNames.WarmasterUser, "Warmaster User", 1);
        await EnsureRoleAsync(dbContext, warmasterSystem, ApplicationRoleNames.WarmasterViewer, "Warmaster Viewer", 2);
        var warmasterAdminRole = await EnsureRoleAsync(
            dbContext,
            warmasterSystem,
            ApplicationRoleNames.WarmasterAdmin,
            "Warmaster Admin",
            0);
        await EnsureRolePermissionsAsync(dbContext, warmasterAdminRole, AppPermissions.WarmasterAdminDefault);

        var seedAdmin = configuration.GetSection(SeedAdminOptions.SectionName).Get<SeedAdminOptions>()
            ?? new SeedAdminOptions();

        if (string.IsNullOrWhiteSpace(seedAdmin.Email) || string.IsNullOrWhiteSpace(seedAdmin.Password))
        {
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(seedAdmin.Email);
        if (existingUser is not null)
        {
            await EnsureGlobalAdminAccessAsync(dbContext, existingUser.Id, globalSystem.Id, globalAdminRole.Id);
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = seedAdmin.Email,
            Email = seedAdmin.Email,
            EmailConfirmed = true,
            DisplayName = string.IsNullOrWhiteSpace(seedAdmin.DisplayName)
                ? seedAdmin.Email
                : seedAdmin.DisplayName
        };

        var createResult = await userManager.CreateAsync(adminUser, seedAdmin.Password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(BuildIdentityErrorMessage("admin user", createResult.Errors));
        }

        await EnsureGlobalAdminAccessAsync(dbContext, adminUser.Id, globalSystem.Id, globalAdminRole.Id);
    }

    private static async Task<SystemDefinition> EnsureSystemAsync(
        MldevDashboardDbContext dbContext,
        string systemKey,
        string label,
        int sortOrder)
    {
        var system = await dbContext.Systems
            .Include(system => system.AccountAccesses)
            .SingleOrDefaultAsync(system => system.SystemKey == systemKey);

        if (system is null)
        {
            system = new SystemDefinition
            {
                SystemKey = systemKey,
                Label = label,
                SortOrder = sortOrder,
                IsActive = true
            };

            dbContext.Systems.Add(system);
            await dbContext.SaveChangesAsync();
            return system;
        }

        system.Label = label;
        system.SortOrder = sortOrder;
        system.IsActive = true;
        await dbContext.SaveChangesAsync();

        return system;
    }

    private static async Task EnsurePermissionsAsync(
        MldevDashboardDbContext dbContext,
        SystemDefinition system,
        PermissionDefinition[] permissionDefinitions)
    {
        foreach (var permissionDefinition in permissionDefinitions)
        {
            var permission = await dbContext.ApplicationPermissions
                .SingleOrDefaultAsync(existingPermission =>
                    existingPermission.PermissionKey == permissionDefinition.Key);

            if (permission is null)
            {
                dbContext.ApplicationPermissions.Add(new ApplicationPermission
                {
                    SystemDefinitionId = system.Id,
                    PermissionKey = permissionDefinition.Key,
                    DisplayName = permissionDefinition.DisplayName,
                    Category = permissionDefinition.Category
                });
                continue;
            }

            permission.SystemDefinitionId = system.Id;
            permission.DisplayName = permissionDefinition.DisplayName;
            permission.Category = permissionDefinition.Category;
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<ApplicationRole> EnsureRoleAsync(
        MldevDashboardDbContext dbContext,
        SystemDefinition system,
        string name,
        string displayName,
        int sortOrder,
        bool isProtected = false)
    {
        var role = await dbContext.ApplicationRoles
            .Include(role => role.RolePermissions)
            .SingleOrDefaultAsync(role => role.Name == name);

        if (role is null)
        {
            role = new ApplicationRole
            {
                SystemDefinitionId = system.Id,
                Name = name,
                DisplayName = displayName,
                IsProtected = isProtected,
                SortOrder = sortOrder
            };

            dbContext.ApplicationRoles.Add(role);
            await dbContext.SaveChangesAsync();
            return role;
        }

        role.SystemDefinitionId = system.Id;
        role.IsProtected = isProtected;
        role.SortOrder = sortOrder;
        await dbContext.SaveChangesAsync();

        return role;
    }

    private static async Task EnsureRolePermissionsAsync(
        MldevDashboardDbContext dbContext,
        ApplicationRole role,
        string[] permissionKeys)
    {
        var permissions = await dbContext.ApplicationPermissions
            .Where(permission => permissionKeys.Contains(permission.PermissionKey))
            .ToArrayAsync();
        var existingPermissionIds = await dbContext.ApplicationRolePermissions
            .Where(rolePermission => rolePermission.ApplicationRoleId == role.Id)
            .Select(rolePermission => rolePermission.ApplicationPermissionId)
            .ToArrayAsync();

        foreach (var permission in permissions)
        {
            if (!existingPermissionIds.Contains(permission.Id))
            {
                dbContext.ApplicationRolePermissions.Add(new ApplicationRolePermission
                {
                    ApplicationRoleId = role.Id,
                    ApplicationPermissionId = permission.Id
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureGlobalAdminAccessAsync(
        MldevDashboardDbContext dbContext,
        Guid accountId,
        int globalSystemId,
        int globalAdminRoleId)
    {
        var hasRole = await dbContext.SystemAccountRoles.AnyAsync(accountRole =>
            accountRole.AccountId == accountId &&
            accountRole.ApplicationRoleId == globalAdminRoleId);
        if (hasRole)
        {
            return;
        }

        dbContext.SystemAccountRoles.Add(new SystemAccountRole
        {
            AccountId = accountId,
            SystemDefinitionId = globalSystemId,
            ApplicationRoleId = globalAdminRoleId
        });
        await dbContext.SaveChangesAsync();
    }

    private static string BuildIdentityErrorMessage(
        string operation,
        IEnumerable<IdentityError> errors)
    {
        var details = string.Join("; ", errors.Select(error => $"{error.Code}: {error.Description}"));
        return $"Unable to seed {operation}. {details}";
    }
}
