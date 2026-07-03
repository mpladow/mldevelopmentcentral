using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Global.Accounts.CreateAccount;
using MldevDashboard.Api.Identity;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Tests.Features.Accounts;

public sealed class AccountHandlerTests
{
    [Fact]
    public async Task CreateAccountHandler_CreatesUserWithRequestedRole()
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
                [ApplicationRoleNames.GlobalAdmin]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("user@example.com", result.Value.Email);

        var user = await userManager.FindByEmailAsync("user@example.com");

        Assert.NotNull(user);
        Assert.True(await dbContext.SystemAccountRoles.AnyAsync(accountRole =>
            accountRole.AccountId == user.Id &&
            accountRole.ApplicationRole.Name == ApplicationRoleNames.GlobalAdmin));
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
            IsProtected = true
        });

        dbContext.Systems.Add(globalSystem);
        await dbContext.SaveChangesAsync();
    }
}
