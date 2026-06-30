namespace MldevDashboard.Api.Common;

public static class ApplicationResultExtensions
{
    public static IResult ToHttpResult<T>(
        this ApplicationResult<T> result,
        Func<T, string>? createdLocationFactory = null)
    {
        return result.Status switch
        {
            ApplicationResultStatus.Success => Results.Ok(result.Value),
            ApplicationResultStatus.Created => Results.Created(
                createdLocationFactory?.Invoke(result.Value!)
                    ?? string.Empty,
                result.Value),
            ApplicationResultStatus.BadRequest => Results.BadRequest(ToErrorBody(result)),
            ApplicationResultStatus.NotFound => Results.NotFound(new { message = result.Message }),
            ApplicationResultStatus.Conflict => Results.Conflict(new { message = result.Message }),
            ApplicationResultStatus.Unauthorized => Results.Unauthorized(),
            _ => Results.Problem("Unhandled application result.")
        };
    }

    private static object ToErrorBody<T>(ApplicationResult<T> result)
    {
        return result.Errors is { Length: > 0 }
            ? new { errors = result.Errors }
            : new { message = result.Message };
    }
}
