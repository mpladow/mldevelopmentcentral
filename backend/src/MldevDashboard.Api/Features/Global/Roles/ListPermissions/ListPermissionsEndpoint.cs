namespace MldevDashboard.Api.Features.Global.Roles.ListPermissions;

public static class ListPermissionsEndpoint
{
    public static async Task<IResult> HandleAsync(
        ListPermissionsHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.HandleAsync(cancellationToken));
    }
}
