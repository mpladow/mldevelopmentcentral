using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Global.Accounts.UpdateAccount;

public static class UpdateAccountEndpoint
{
    public static async Task<IResult> HandleAsync(
        Guid id,
        UpdateAccountRequest request,
        UpdateAccountHandler handler,
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

        var result = await handler.HandleAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }
}
