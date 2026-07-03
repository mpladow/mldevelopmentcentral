using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Global.Accounts.CreateAccount;
using MldevDashboard.Api.Features.Global.Accounts.ListAccounts;
using MldevDashboard.Api.Features.Global.Accounts.UpdateAccount;

namespace MldevDashboard.Api.Features.Global.Accounts;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts")
            .WithTags("Accounts")
            .RequireAuthorization(policy => policy.RequirePermission(AppPermissions.GlobalAccountsManage));

        group.MapGet("/", ListAccountsEndpoint.HandleAsync);
        group.MapPost("/", CreateAccountEndpoint.HandleAsync);
        group.MapPut("/{id:guid}", UpdateAccountEndpoint.HandleAsync);

        return app;
    }
}
