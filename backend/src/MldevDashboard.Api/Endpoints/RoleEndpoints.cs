using MldevDashboard.Application.Common;

namespace MldevDashboard.Api.Endpoints;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
            .WithTags("Roles")
            .RequireAuthorization(policy => policy.RequireRole(ApplicationRoles.Admin));

        group.MapGet("/", () => Results.Ok(ApplicationRoles.All));

        return app;
    }
}
