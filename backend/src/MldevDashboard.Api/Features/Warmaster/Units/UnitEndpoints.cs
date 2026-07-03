using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Warmaster.Units.ListUnits;

namespace MldevDashboard.Api.Features.Warmaster.Units;

public static class UnitEndpoints
{
    public static IEndpointRouteBuilder MapUnitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/warmaster/units")
            .WithTags("Warmaster Units")
            .RequireAuthorization(policy => policy.RequirePermission(AppPermissions.WarmasterUnitsView));

        group.MapGet("/", ListUnitsEndpoint.HandleAsync);

        return app;
    }
}
