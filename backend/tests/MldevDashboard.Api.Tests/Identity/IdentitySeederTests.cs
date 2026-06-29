using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MldevDashboard.Api.Identity;
using MldevDashboard.Application.Common;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Api.Tests.Identity;

public sealed class IdentitySeederTests
{
    [Fact]
    public async Task SeedIdentityAsync_CreatesConfiguredRoles()
    {
        var services = CreateServices();
        var configuration = CreateConfiguration();

        await services.SeedIdentityAsync(configuration);

        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in ApplicationRoles.All)
        {
            Assert.True(await roleManager.RoleExistsAsync(role), $"Expected role {role} to exist.");
        }
    }

    [Fact]
    public async Task SeedIdentityAsync_CreatesConfiguredAdminUser()
    {
        var services = CreateServices();
        var configuration = CreateConfiguration();

        await services.SeedIdentityAsync(configuration);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync("ml.development.2022@gmail.com");

        Assert.NotNull(admin);
        Assert.Equal("ML Development Admin", admin.DisplayName);
        Assert.True(await userManager.IsInRoleAsync(admin, ApplicationRoles.Admin));
    }

    [Fact]
    public async Task SeedIdentityAsync_CreatesGlobalSystem()
    {
        var services = CreateServices();
        var configuration = CreateConfiguration(("SeedAdmin:Password", string.Empty));

        await services.SeedIdentityAsync(configuration);

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MldevDashboardDbContext>();
        var globalSystem = await dbContext.Systems.SingleOrDefaultAsync(system => system.SystemKey == "global");

        Assert.NotNull(globalSystem);
        Assert.Equal("Global", globalSystem.Label);
        Assert.Equal(0, globalSystem.SortOrder);
        Assert.True(globalSystem.IsActive);
    }

    [Fact]
    public async Task SeedIdentityAsync_AssignsGlobalSystemToConfiguredAdminUser()
    {
        var services = CreateServices();
        var configuration = CreateConfiguration();

        await services.SeedIdentityAsync(configuration);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<MldevDashboardDbContext>();
        var admin = await userManager.FindByEmailAsync("ml.development.2022@gmail.com");

        Assert.NotNull(admin);
        Assert.True(await dbContext.SystemAccountAccesses.AnyAsync(access => access.AccountId == admin.Id));
    }

    [Fact]
    public async Task SeedIdentityAsync_CanRunMultipleTimesWithoutDuplicatingGlobalSystem()
    {
        var services = CreateServices();
        var configuration = CreateConfiguration();

        await services.SeedIdentityAsync(configuration);
        await services.SeedIdentityAsync(configuration);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<MldevDashboardDbContext>();
        var admin = await userManager.FindByEmailAsync("ml.development.2022@gmail.com");

        Assert.NotNull(admin);
        Assert.Equal(1, await dbContext.Systems.CountAsync(system => system.SystemKey == "global"));
        Assert.Equal(1, await dbContext.SystemAccountAccesses.CountAsync(access => access.AccountId == admin.Id));
    }

    [Fact]
    public async Task SeedIdentityAsync_DoesNotCreateAdminWhenPasswordIsMissing()
    {
        var services = CreateServices();
        var configuration = CreateConfiguration(("SeedAdmin:Password", string.Empty));

        await services.SeedIdentityAsync(configuration);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync("ml.development.2022@gmail.com");

        Assert.Null(admin);
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        var databaseName = Guid.NewGuid().ToString();

        services.AddLogging(builder => builder.AddDebug());
        services.AddDbContext<MldevDashboardDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

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

    private static IConfiguration CreateConfiguration(params (string Key, string Value)[] overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["SeedAdmin:Email"] = "ml.development.2022@gmail.com",
            ["SeedAdmin:Password"] = "UnitTestPassword1",
            ["SeedAdmin:DisplayName"] = "ML Development Admin"
        };

        foreach (var (key, value) in overrides)
        {
            values[key] = value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
