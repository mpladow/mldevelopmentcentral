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

        var globalSystem = await EnsureGlobalSystemAsync(dbContext);
        await EnsureGlobalPermissionsAsync(dbContext, globalSystem);
        var globalAdminRole = await EnsureGlobalAdminRoleAsync(dbContext, globalSystem);
        await EnsureGlobalAdminMinimumPermissionsAsync(dbContext, globalAdminRole);

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

    private static async Task<SystemDefinition> EnsureGlobalSystemAsync(MldevDashboardDbContext dbContext)
    {
        var globalSystem = await dbContext.Systems
            .Include(system => system.AccountAccesses)
            .SingleOrDefaultAsync(system => system.SystemKey == "global");

        if (globalSystem is null)
        {
            globalSystem = new SystemDefinition
            {
                SystemKey = "global",
                Label = "Global",
                SortOrder = 0,
                IsActive = true
            };

            dbContext.Systems.Add(globalSystem);
            await dbContext.SaveChangesAsync();
        }

        return globalSystem;
    }

    private static async Task EnsureGlobalPermissionsAsync(
        MldevDashboardDbContext dbContext,
        SystemDefinition globalSystem)
    {
        foreach (var permissionDefinition in AppPermissions.Global)
        {
            var permission = await dbContext.ApplicationPermissions
                .SingleOrDefaultAsync(existingPermission =>
                    existingPermission.PermissionKey == permissionDefinition.Key);

            if (permission is null)
            {
                dbContext.ApplicationPermissions.Add(new ApplicationPermission
                {
                    SystemDefinitionId = globalSystem.Id,
                    PermissionKey = permissionDefinition.Key,
                    DisplayName = permissionDefinition.DisplayName,
                    Category = permissionDefinition.Category
                });
                continue;
            }

            permission.SystemDefinitionId = globalSystem.Id;
            permission.DisplayName = permissionDefinition.DisplayName;
            permission.Category = permissionDefinition.Category;
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<ApplicationRole> EnsureGlobalAdminRoleAsync(
        MldevDashboardDbContext dbContext,
        SystemDefinition globalSystem)
    {
        var globalAdminRole = await dbContext.ApplicationRoles
            .Include(role => role.RolePermissions)
            .SingleOrDefaultAsync(role => role.Name == ApplicationRoleNames.GlobalAdmin);

        if (globalAdminRole is null)
        {
            globalAdminRole = new ApplicationRole
            {
                SystemDefinitionId = globalSystem.Id,
                Name = ApplicationRoleNames.GlobalAdmin,
                DisplayName = "Global Admin",
                IsProtected = true,
                SortOrder = 0
            };

            dbContext.ApplicationRoles.Add(globalAdminRole);
            await dbContext.SaveChangesAsync();
        }

        globalAdminRole.SystemDefinitionId = globalSystem.Id;
        globalAdminRole.IsProtected = true;
        globalAdminRole.SortOrder = 0;
        await dbContext.SaveChangesAsync();

        return globalAdminRole;
    }

    private static async Task EnsureGlobalAdminMinimumPermissionsAsync(
        MldevDashboardDbContext dbContext,
        ApplicationRole globalAdminRole)
    {
        var minimumPermissions = await dbContext.ApplicationPermissions
            .Where(permission => AppPermissions.GlobalAdminMinimum.Contains(permission.PermissionKey))
            .ToArrayAsync();
        var existingPermissionIds = await dbContext.ApplicationRolePermissions
            .Where(rolePermission => rolePermission.ApplicationRoleId == globalAdminRole.Id)
            .Select(rolePermission => rolePermission.ApplicationPermissionId)
            .ToArrayAsync();

        foreach (var permission in minimumPermissions)
        {
            if (!existingPermissionIds.Contains(permission.Id))
            {
                dbContext.ApplicationRolePermissions.Add(new ApplicationRolePermission
                {
                    ApplicationRoleId = globalAdminRole.Id,
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
