namespace MldevDashboard.Api.Features.Warmaster.Units.ListUnits;

public static class ListUnitsEndpoint
{
    public static async Task<IResult> HandleAsync(
        ListUnitsHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.HandleAsync(cancellationToken));
    }
}
