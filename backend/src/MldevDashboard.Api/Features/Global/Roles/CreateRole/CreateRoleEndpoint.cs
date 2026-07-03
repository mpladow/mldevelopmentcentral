using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Global.Roles.CreateRole;

public static class CreateRoleEndpoint
{
    public static async Task<IResult> HandleAsync(
        CreateRoleRequest request,
        CreateRoleHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(role => $"/api/roles/{role.Id}");
    }
}
