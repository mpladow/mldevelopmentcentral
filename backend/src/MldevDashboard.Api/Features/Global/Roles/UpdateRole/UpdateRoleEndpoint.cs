using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Global.Roles.UpdateRole;

public static class UpdateRoleEndpoint
{
    public static async Task<IResult> HandleAsync(
        int id,
        UpdateRoleRequest request,
        UpdateRoleHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }
}
