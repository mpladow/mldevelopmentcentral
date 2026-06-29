using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Application.Common;
using MldevDashboard.Domain.Systems;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Api.Identity;

public static class IdentitySeeder
{
    public static async Task SeedIdentityAsync(
        this IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<MldevDashboardDbContext>();

        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(BuildIdentityErrorMessage("role", roleResult.Errors));
                }
            }
        }

        await EnsureGlobalSystemAsync(dbContext);

        var seedAdmin = configuration.GetSection(SeedAdminOptions.SectionName).Get<SeedAdminOptions>()
            ?? new SeedAdminOptions();

        if (string.IsNullOrWhiteSpace(seedAdmin.Email) || string.IsNullOrWhiteSpace(seedAdmin.Password))
        {
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(seedAdmin.Email);
        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, ApplicationRoles.Admin))
            {
                var addRoleResult = await userManager.AddToRoleAsync(existingUser, ApplicationRoles.Admin);
                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(BuildIdentityErrorMessage("admin role assignment", addRoleResult.Errors));
                }
            }

            await EnsureGlobalSystemAccessAsync(dbContext, existingUser.Id);
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

        var adminRoleResult = await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
        if (!adminRoleResult.Succeeded)
        {
            throw new InvalidOperationException(BuildIdentityErrorMessage("admin role assignment", adminRoleResult.Errors));
        }

        await EnsureGlobalSystemAccessAsync(dbContext, adminUser.Id);
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

    private static async Task EnsureGlobalSystemAccessAsync(
        MldevDashboardDbContext dbContext,
        Guid accountId)
    {
        var globalSystem = await EnsureGlobalSystemAsync(dbContext);

        if (!globalSystem.AccountAccesses.Any(access => access.AccountId == accountId))
        {
            globalSystem.AccountAccesses.Add(new SystemAccountAccess { AccountId = accountId });
            await dbContext.SaveChangesAsync();
        }
    }

    private static string BuildIdentityErrorMessage(
        string operation,
        IEnumerable<IdentityError> errors)
    {
        var details = string.Join("; ", errors.Select(error => $"{error.Code}: {error.Description}"));
        return $"Unable to seed {operation}. {details}";
    }
}
