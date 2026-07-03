using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Warmaster.Factions.ListFactions;

namespace MldevDashboard.Api.Features.Warmaster.Factions;

public static class FactionEndpoints
{
    public static IEndpointRouteBuilder MapFactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/warmaster/factions")
            .WithTags("Warmaster Factions")
            .RequireAuthorization(policy => policy.RequirePermission(AppPermissions.WarmasterFactionsView));

        group.MapGet("/", ListFactionsEndpoint.HandleAsync);

        return app;
    }
}
