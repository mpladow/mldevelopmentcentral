namespace MldevDashboard.Api.Features.Global.Accounts.ListAccounts;

public static class ListAccountsEndpoint
{
    public static async Task<IResult> HandleAsync(
        ListAccountsHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.HandleAsync(cancellationToken));
    }
}
