namespace MldevDashboard.Api.Features.Global.Roles.ListRoles;

public static class ListRolesEndpoint
{
    public static async Task<IResult> HandleAsync(
        ListRolesHandler handler,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await handler.HandleAsync(cancellationToken));
    }
}
