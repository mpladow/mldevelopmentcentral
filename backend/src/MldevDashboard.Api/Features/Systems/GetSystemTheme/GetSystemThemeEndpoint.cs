using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Systems.GetSystemTheme;

public static class GetSystemThemeEndpoint
{
    public static async Task<IResult> HandleAsync(
        int id,
        GetSystemThemeHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);
        return result.ToHttpResult();
    }
}
