using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Accounts.CreateAccount;
using MldevDashboard.Api.Features.Accounts.ListAccounts;
using MldevDashboard.Api.Features.Accounts.UpdateAccount;

namespace MldevDashboard.Api.Features.Accounts;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts")
            .WithTags("Accounts")
            .RequireAuthorization(policy => policy.RequireRole(ApplicationRoles.Admin));

        group.MapGet("/", ListAccountsEndpoint.HandleAsync);
        group.MapPost("/", CreateAccountEndpoint.HandleAsync);
        group.MapPut("/{id:guid}", UpdateAccountEndpoint.HandleAsync);

        return app;
    }
}
