using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Global.Systems.UpdateSystem;

public static class UpdateSystemEndpoint
{
    public static async Task<IResult> HandleAsync(
        int id,
        UpdateSystemRequest request,
        UpdateSystemHandler handler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Label))
        {
            return Results.BadRequest(new { message = "System name is required." });
        }

        var result = await handler.HandleAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }
}
