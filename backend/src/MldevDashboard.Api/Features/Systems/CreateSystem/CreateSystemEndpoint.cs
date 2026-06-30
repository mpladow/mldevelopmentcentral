using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Systems.CreateSystem;

public static class CreateSystemEndpoint
{
    public static async Task<IResult> HandleAsync(
        CreateSystemRequest request,
        CreateSystemHandler handler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Label))
        {
            return Results.BadRequest(new { message = "System name is required." });
        }

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(system => $"/api/systems/{system.Id}");
    }
}
