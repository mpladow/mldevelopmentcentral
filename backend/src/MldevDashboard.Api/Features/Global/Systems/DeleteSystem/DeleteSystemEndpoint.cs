using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Global.Systems.DeleteSystem;

public static class DeleteSystemEndpoint
{
    public static async Task<IResult> HandleAsync(
        int id,
        DeleteSystemHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);

        return result.Status switch
        {
            ApplicationResultStatus.Success => Results.NoContent(),
            ApplicationResultStatus.BadRequest => Results.BadRequest(new { message = result.Message }),
            ApplicationResultStatus.NotFound => Results.NotFound(new { message = result.Message }),
            _ => Results.Problem("Unhandled application result.")
        };
    }
}
