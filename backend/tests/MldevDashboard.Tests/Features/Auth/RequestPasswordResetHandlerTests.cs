using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Auth.RequestPasswordReset;
using MldevDashboard.Api.Messaging;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Tests.Features.Auth;

public sealed class RequestPasswordResetHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsGenericSuccessAndDoesNotPublishWhenAccountDoesNotExist()
    {
        var services = CreateServices();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var publisher = new CapturingPasswordResetEmailPublisher();
        var handler = CreateHandler(userManager, publisher);

        var result = await handler.HandleAsync(
            new RequestPasswordResetRequest("missing@example.com"),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(
            "If an account exists, reset instructions will be emailed to the provided address",
            result.Value.Message);
        Assert.Empty(publisher.Messages);
    }

    [Fact]
    public async Task HandleAsync_PublishesPasswordResetEmailRequestWhenAccountExists()
    {
        var services = CreateServices();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var publisher = new CapturingPasswordResetEmailPublisher();
        var handler = CreateHandler(userManager, publisher);
        var user = new ApplicationUser
        {
            UserName = "user@example.com",
            Email = "user@example.com",
            EmailConfirmed = true,
            DisplayName = "User Example"
        };
        var createResult = await userManager.CreateAsync(user, "Password1");
        Assert.True(createResult.Succeeded);
        var requestedAfterUtc = DateTimeOffset.UtcNow;

        var result = await handler.HandleAsync(
            new RequestPasswordResetRequest(" user@example.com "),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Success, result.Status);
        var message = Assert.Single(publisher.Messages);
        Assert.Equal("1", message.SchemaVersion);
        Assert.True(Guid.TryParse(message.CorrelationId, out _));
        Assert.Equal("user@example.com", message.Email);
        Assert.True(message.RequestedAtUtc >= requestedAfterUtc);
        Assert.True(message.RequestedAtUtc <= DateTimeOffset.UtcNow);

        var resetUri = new Uri(message.ResetUrl);
        Assert.Equal("https://app.example.com/reset-password", resetUri.GetLeftPart(UriPartial.Path));

        var query = QueryHelpers.ParseQuery(resetUri.Query);
        Assert.Equal("user@example.com", query["email"]);
        Assert.True(query.TryGetValue("token", out var encodedToken));
        Assert.False(string.IsNullOrWhiteSpace(encodedToken));

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken!));
        var isTokenValid = await userManager.VerifyUserTokenAsync(
            user,
            TokenOptions.DefaultProvider,
            UserManager<ApplicationUser>.ResetPasswordTokenPurpose,
            decodedToken);

        Assert.True(isTokenValid);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToPublisher()
    {
        var services = CreateServices();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var publisher = new CapturingPasswordResetEmailPublisher();
        var handler = CreateHandler(userManager, publisher);
        var user = new ApplicationUser
        {
            UserName = "user@example.com",
            Email = "user@example.com",
            EmailConfirmed = true,
            DisplayName = "User Example"
        };
        var createResult = await userManager.CreateAsync(user, "Password1");
        Assert.True(createResult.Succeeded);
        using var cancellationTokenSource = new CancellationTokenSource();

        await handler.HandleAsync(
            new RequestPasswordResetRequest("user@example.com"),
            cancellationTokenSource.Token);

        Assert.Equal(cancellationTokenSource.Token, publisher.CancellationToken);
    }

    private static RequestPasswordResetHandler CreateHandler(
        UserManager<ApplicationUser> userManager,
        CapturingPasswordResetEmailPublisher publisher)
    {
        return new RequestPasswordResetHandler(
            userManager,
            publisher,
            Options.Create(new RequestPasswordResetOptions
            {
                ResetPasswordUrl = "https://app.example.com/reset-password"
            }));
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        var databaseName = Guid.NewGuid().ToString();

        services.AddLogging();
        services
            .AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(
                Path.Combine(AppContext.BaseDirectory, "data-protection-keys", databaseName)));
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
            .AddEntityFrameworkStores<MldevDashboardDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }

    private sealed class CapturingPasswordResetEmailPublisher : IPasswordResetEmailPublisher
    {
        public List<PasswordResetEmailRequestedMessage> Messages { get; } = [];

        public CancellationToken CancellationToken { get; private set; }

        public Task PublishAsync(
            PasswordResetEmailRequestedMessage message,
            CancellationToken cancellationToken)
        {
            Messages.Add(message);
            CancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }
}
