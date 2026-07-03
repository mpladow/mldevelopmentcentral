using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Global.Systems;
using MldevDashboard.Api.Features.Global.Systems.CreateSystem;
using MldevDashboard.Api.Features.Global.Systems.GetSystemTheme;
using MldevDashboard.Api.Features.Global.Systems.UpdateSystemTheme;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Tests.Features.Systems;

public sealed class SystemHandlerTests
{
    [Fact]
    public async Task CreateSystemHandler_CreatesSystemWithGeneratedKeyAndAccountAccess()
    {
        var services = CreateServices();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<MldevDashboardDbContext>();
        var account = await CreateUserAsync(userManager);
        var handler = new CreateSystemHandler(dbContext, userManager);

        var result = await handler.HandleAsync(
            new CreateSystemRequest("War Machine", [new SystemAccountAssignmentRequest(account.Id, "WarMachineAdmin")]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("war-machine", result.Value.SystemKey);
        Assert.Single(result.Value.Accounts);
        Assert.Equal("WarMachineAdmin", result.Value.Accounts.Single().Role);
        Assert.True(await dbContext.SystemAccountRoles.AnyAsync(access =>
            access.AccountId == account.Id &&
            access.ApplicationRole.Name == "WarMachineAdmin"));
    }

    [Fact]
    public async Task CreateSystemHandler_ReturnsBadRequestWhenAssignedAccountDoesNotExist()
    {
        var services = CreateServices();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<MldevDashboardDbContext>();
        var handler = new CreateSystemHandler(dbContext, userManager);

        var result = await handler.HandleAsync(
            new CreateSystemRequest("War Machine", [new SystemAccountAssignmentRequest(Guid.NewGuid(), "WarMachineUser")]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.BadRequest, result.Status);
        Assert.Equal("One or more assigned accounts do not exist.", result.Message);
    }

    [Fact]
    public async Task CreateSystemHandler_ReturnsBadRequestWhenAssignedAccountRoleIsInvalid()
    {
        var services = CreateServices();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<MldevDashboardDbContext>();
        var account = await CreateUserAsync(userManager);
        var handler = new CreateSystemHandler(dbContext, userManager);

        var result = await handler.HandleAsync(
            new CreateSystemRequest("War Machine", [new SystemAccountAssignmentRequest(account.Id, "Owner")]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.BadRequest, result.Status);
        Assert.Equal("One or more assigned account roles are invalid.", result.Message);
    }

    [Fact]
    public async Task CreateSystemHandler_ReturnsConflictWhenGeneratedKeyAlreadyExists()
    {
        var services = CreateServices();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<MldevDashboardDbContext>();
        var account = await CreateUserAsync(userManager);
        dbContext.Systems.Add(new SystemDefinition { SystemKey = "war-machine", Label = "War Machine" });
        await dbContext.SaveChangesAsync();
        var handler = new CreateSystemHandler(dbContext, userManager);

        var result = await handler.HandleAsync(
            new CreateSystemRequest("War Machine", [new SystemAccountAssignmentRequest(account.Id, "WarMachineUser")]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Conflict, result.Status);
        Assert.Equal("A system already exists with this name.", result.Message);
    }

    [Fact]
    public async Task SystemThemeHandlers_SaveAndReadThemeSettings()
    {
        var services = CreateServices();
        var dbContext = services.GetRequiredService<MldevDashboardDbContext>();
        var system = new SystemDefinition { SystemKey = "global", Label = "Global" };
        dbContext.Systems.Add(system);
        await dbContext.SaveChangesAsync();
        var updateHandler = new UpdateSystemThemeHandler(dbContext);
        var getHandler = new GetSystemThemeHandler(dbContext);

        var updateResult = await updateHandler.HandleAsync(
            system.Id,
            new UpdateSystemThemeRequest("#0f172a", "#0f766e", "#f8fafc", "#ffffff", "#111827", 10),
            CancellationToken.None);
        var getResult = await getHandler.HandleAsync(system.Id, CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Success, updateResult.Status);
        Assert.NotNull(updateResult.Value);
        Assert.Equal("#0f172a", updateResult.Value.PrimaryColor);
        Assert.Equal(10, updateResult.Value.BorderRadius);
        Assert.Equal(ApplicationResultStatus.Success, getResult.Status);
        Assert.NotNull(getResult.Value);
        Assert.Equal("#0f172a", getResult.Value.PrimaryColor);
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

    private static async Task<ApplicationUser> CreateUserAsync(UserManager<ApplicationUser> userManager)
    {
        var user = new ApplicationUser
        {
            UserName = "user@example.com",
            Email = "user@example.com",
            EmailConfirmed = true,
            DisplayName = "User Example"
        };

        var result = await userManager.CreateAsync(user, "Password1");
        Assert.True(result.Succeeded);

        return user;
    }
}
