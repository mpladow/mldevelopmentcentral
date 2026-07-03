using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Global.Roles.CreateRole;
using MldevDashboard.Api.Features.Global.Roles.ListRoles;
using MldevDashboard.Api.Features.Global.Roles.ListPermissions;
using MldevDashboard.Api.Features.Global.Roles.UpdateRole;

namespace MldevDashboard.Api.Features.Global.Roles;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
            .WithTags("Roles")
            .RequireAuthorization(policy => policy.RequirePermission(AppPermissions.GlobalRolesManage));

        group.MapGet("/", ListRolesEndpoint.HandleAsync);
        group.MapGet("/permissions", ListPermissionsEndpoint.HandleAsync);
        group.MapPost("/", CreateRoleEndpoint.HandleAsync);
        group.MapPut("/{id:int}", UpdateRoleEndpoint.HandleAsync);

        return app;
    }
}
