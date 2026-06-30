using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Accounts.CreateAccount;

public static class CreateAccountEndpoint
{
    public static async Task<IResult> HandleAsync(
        CreateAccountRequest request,
        CreateAccountHandler handler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Results.BadRequest(new { message = "Email is required." });
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Results.BadRequest(new { message = "Display name is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { message = "Password is required." });
        }

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(account => $"/api/accounts/{account.Id}");
    }
}
