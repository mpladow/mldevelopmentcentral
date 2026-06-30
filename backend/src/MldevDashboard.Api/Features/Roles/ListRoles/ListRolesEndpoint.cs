namespace MldevDashboard.Api.Features.Roles.ListRoles;

public static class ListRolesEndpoint
{
    public static IResult Handle(ListRolesHandler handler)
    {
        return Results.Ok(handler.Handle());
    }
}
