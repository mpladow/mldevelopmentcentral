namespace MldevDashboard.Api.Features.Warmaster.Factions.ListFactions;

public static class ListFactionsEndpoint
{
    public static async Task<IResult> HandleAsync(
        ListFactionsHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.HandleAsync(cancellationToken));
    }
}
