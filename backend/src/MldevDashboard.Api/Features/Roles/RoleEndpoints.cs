using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Roles.ListRoles;

namespace MldevDashboard.Api.Features.Roles;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
            .WithTags("Roles")
            .RequireAuthorization(policy => policy.RequireRole(ApplicationRoles.Admin));

        group.MapGet("/", ListRolesEndpoint.Handle);

        return app;
    }
}
