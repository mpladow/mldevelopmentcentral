using MldevDashboard.Api.Features.Auth.GetCurrentUser;
using MldevDashboard.Api.Features.Auth.Login;

namespace MldevDashboard.Api.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/login", LoginEndpoint.HandleAsync)
            .AllowAnonymous();

        group.MapGet("/me", GetCurrentUserEndpoint.HandleAsync)
            .RequireAuthorization();

        return app;
    }
}
