using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Systems.CreateSystem;
using MldevDashboard.Api.Features.Systems.GetSystemTheme;
using MldevDashboard.Api.Features.Systems.ListAvailableSystems;
using MldevDashboard.Api.Features.Systems.ListSystems;
using MldevDashboard.Api.Features.Systems.UpdateSystem;
using MldevDashboard.Api.Features.Systems.UpdateSystemTheme;

namespace MldevDashboard.Api.Features.Systems;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/systems")
            .WithTags("Systems")
            .RequireAuthorization();

        group.MapGet("/available", ListAvailableSystemsEndpoint.HandleAsync);

        var adminGroup = group.MapGroup("")
            .RequireAuthorization(policy => policy.RequireRole(ApplicationRoles.Admin));

        adminGroup.MapGet("/", ListSystemsEndpoint.HandleAsync);
        adminGroup.MapPost("/", CreateSystemEndpoint.HandleAsync);
        adminGroup.MapPut("/{id:int}", UpdateSystemEndpoint.HandleAsync);
        adminGroup.MapGet("/{id:int}/theme", GetSystemThemeEndpoint.HandleAsync);
        adminGroup.MapPut("/{id:int}/theme", UpdateSystemThemeEndpoint.HandleAsync);

        return app;
    }
}
