using MldevDashboard.Application.Accounts;
using MldevDashboard.Application.Common;

namespace MldevDashboard.Api.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts")
            .WithTags("Accounts")
            .RequireAuthorization(policy => policy.RequireRole(ApplicationRoles.Admin));

        group.MapGet("/", ListAccountsAsync);
        group.MapPost("/", CreateAccountAsync);
        group.MapPut("/{id:guid}", UpdateAccountAsync);

        return app;
    }

    private static async Task<IResult> ListAccountsAsync(
        IAccountService accountService,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await accountService.ListAccountsAsync(cancellationToken));
    }

    private static async Task<IResult> CreateAccountAsync(
        CreateAccountRequest request,
        IAccountService accountService,
        CancellationToken cancellationToken)
    {
        var result = await accountService.CreateAccountAsync(request, cancellationToken);
        return result.ToHttpResult(account => $"/api/accounts/{account.Id}");
    }

    private static async Task<IResult> UpdateAccountAsync(
        Guid id,
        UpdateAccountRequest request,
        IAccountService accountService,
        CancellationToken cancellationToken)
    {
        var result = await accountService.UpdateAccountAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }
}
