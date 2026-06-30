using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Auth.Login;

public static class LoginEndpoint
{
    public static async Task<IResult> HandleAsync(
        LoginRequest request,
        LoginHandler handler)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Results.BadRequest(new { message = "Email is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { message = "Password is required." });
        }

        var result = await handler.HandleAsync(request);
        return result.ToHttpResult();
    }
}
