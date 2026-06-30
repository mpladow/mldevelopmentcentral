namespace MldevDashboard.Api.Features.Systems.ListSystems;

public static class ListSystemsEndpoint
{
    public static async Task<IResult> HandleAsync(
        ListSystemsHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.HandleAsync(cancellationToken));
    }
}
