using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Accounts;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Tests.Features.Accounts;

public sealed class AccountEndpointsTests
{
    [Fact]
    public async Task CreateAccountResultAsync_CreatesUserWithRequestedRole()
    {
        var services = CreateServices();
        await SeedRolesAsync(services);
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var result = await AccountEndpoints.CreateAccountResultAsync(
            new CreateAccountRequest(
                "user@example.com",
                "User Example",
                "Password1",
                [ApplicationRoles.User]),
            userManager,
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("user@example.com", result.Value.Email);

        var user = await userManager.FindByEmailAsync("user@example.com");

        Assert.NotNull(user);
        Assert.True(await userManager.IsInRoleAsync(user, ApplicationRoles.User));
    }

    [Fact]
    public async Task CreateAccountResultAsync_ReturnsBadRequestForUnknownRole()
    {
        var services = CreateServices();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var result = await AccountEndpoints.CreateAccountResultAsync(
            new CreateAccountRequest(
                "user@example.com",
                "User Example",
                "Password1",
                ["Unknown"]),
            userManager,
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
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in ApplicationRoles.All)
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }
}
