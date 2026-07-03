using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Global.Accounts.CreateAccount;
using MldevDashboard.Api.Features.Global.Accounts.UpdateAccount;
using MldevDashboard.Api.Identity;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Tests.Features.Accounts;

public sealed class AccountHandlerTests
{
    [Fact]
    public async Task CreateAccountHandler_CreatesUserWithRequestedRoles()
    {
        var services = CreateServices();
        await SeedRolesAsync(services);
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<MldevDashboardDbContext>();
        var appAuthorizationService = services.GetRequiredService<AppAuthorizationService>();
        var handler = new CreateAccountHandler(userManager, dbContext, appAuthorizationService);

        var result = await handler.HandleAsync(
            new CreateAccountRequest(
                "user@example.com",
                "User Example",
                "Password1",
                [ApplicationRoleNames.GlobalAdmin, ApplicationRoleNames.WarmasterAdmin]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("user@example.com", result.Value.Email);

        var user = await userManager.FindByEmailAsync("user@example.com");

        Assert.NotNull(user);
        var assignedRoles = await dbContext.SystemAccountRoles
            .Where(accountRole => accountRole.AccountId == user.Id)
            .Select(accountRole => accountRole.ApplicationRole.Name)
            .OrderBy(role => role)
            .ToArrayAsync();

        Assert.Equal(
            [ApplicationRoleNames.GlobalAdmin, ApplicationRoleNames.WarmasterAdmin],
            assignedRoles);
    }

    [Fact]
    public async Task UpdateAccountHandler_ReplacesExistingRolesWithRequestedRoles()
    {
        var services = CreateServices();
        await SeedRolesAsync(services);
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<MldevDashboardDbContext>();
        var appAuthorizationService = services.GetRequiredService<AppAuthorizationService>();
        var handler = new UpdateAccountHandler(userManager, dbContext, appAuthorizationService);
        var user = new ApplicationUser
        {
            UserName = "user@example.com",
            Email = "user@example.com",
            EmailConfirmed = true,
            DisplayName = "User Example"
        };
        var createResult = await userManager.CreateAsync(user, "Password1");
        Assert.True(createResult.Succeeded);

        var initialRole = await dbContext.ApplicationRoles
            .SingleAsync(role => role.Name == ApplicationRoleNames.GlobalViewer);
        dbContext.SystemAccountRoles.Add(new SystemAccountRole
        {
            AccountId = user.Id,
            SystemDefinitionId = initialRole.SystemDefinitionId,
            ApplicationRoleId = initialRole.Id
        });
        await dbContext.SaveChangesAsync();

        var result = await handler.HandleAsync(
            user.Id,
            new UpdateAccountRequest(
                "updated@example.com",
                "Updated Example",
                [ApplicationRoleNames.GlobalAdmin, ApplicationRoleNames.WarmasterAdmin]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("updated@example.com", result.Value.Email);

        var assignedRoles = await dbContext.SystemAccountRoles
            .Where(accountRole => accountRole.AccountId == user.Id)
            .Select(accountRole => accountRole.ApplicationRole.Name)
            .OrderBy(role => role)
            .ToArrayAsync();

        Assert.Equal(
            [ApplicationRoleNames.GlobalAdmin, ApplicationRoleNames.WarmasterAdmin],
            assignedRoles);
    }

    [Fact]
    public async Task CreateAccountHandler_ReturnsBadRequestForUnknownRole()
    {
        var services = CreateServices();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<MldevDashboardDbContext>();
        var appAuthorizationService = services.GetRequiredService<AppAuthorizationService>();
        var handler = new CreateAccountHandler(userManager, dbContext, appAuthorizationService);

        var result = await handler.HandleAsync(
            new CreateAccountRequest(
                "user@example.com",
                "User Example",
                "Password1",
                ["Unknown"]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.BadRequest, result.Status);
        Assert.Contains("Unknown", result.Message);
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        var databaseName = Guid.NewGuid().ToString();

        services.AddLogging(builder => builder.AddDebug());
        services.AddDbContext<MldevDashboardDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddScoped<AppAuthorizationService>();

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<MldevDashboardDbContext>();

        return services.BuildServiceProvider();
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<MldevDashboardDbContext>();
        var globalSystem = new SystemDefinition
        {
            SystemKey = "global",
            Label = "Global",
            SortOrder = 0,
            IsActive = true
        };

        globalSystem.Roles.Add(new ApplicationRole
        {
            Name = ApplicationRoleNames.GlobalAdmin,
            DisplayName = "Global Admin",
            IsProtected = true,
            SortOrder = 0
        });
        globalSystem.Roles.Add(new ApplicationRole
        {
            Name = ApplicationRoleNames.GlobalViewer,
            DisplayName = "Global Viewer",
            IsProtected = true,
            SortOrder = 2
        });

        var warmasterSystem = new SystemDefinition
        {
            SystemKey = "warmaster",
            Label = "Warmaster",
            SortOrder = 1,
            IsActive = true
        };
        warmasterSystem.Roles.Add(new ApplicationRole
        {
            Name = ApplicationRoleNames.WarmasterAdmin,
            DisplayName = "Warmaster Admin",
            IsProtected = true,
            SortOrder = 0
        });

        dbContext.Systems.Add(globalSystem);
        dbContext.Systems.Add(warmasterSystem);
        await dbContext.SaveChangesAsync();
    }
}
