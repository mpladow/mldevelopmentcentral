using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Systems.UpdateSystemTheme;

public static class UpdateSystemThemeEndpoint
{
    public static async Task<IResult> HandleAsync(
        int id,
        UpdateSystemThemeRequest request,
        UpdateSystemThemeHandler handler,
        CancellationToken cancellationToken)
    {
        if (HasMissingColor(request))
        {
            return Results.BadRequest(new { message = "Theme colors are required." });
        }

        if (request.BorderRadius < 0 || request.BorderRadius > 24)
        {
            return Results.BadRequest(new { message = "Theme border radius must be between 0 and 24." });
        }

        var result = await handler.HandleAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }

    private static bool HasMissingColor(UpdateSystemThemeRequest request)
    {
        return string.IsNullOrWhiteSpace(request.PrimaryColor)
            || string.IsNullOrWhiteSpace(request.SecondaryColor)
            || string.IsNullOrWhiteSpace(request.BackgroundColor)
            || string.IsNullOrWhiteSpace(request.SurfaceColor)
            || string.IsNullOrWhiteSpace(request.TextColor);
    }
}
