namespace MldevDashboard.Api.Features.Global.Systems.UpdateSystemTheme;

public sealed record UpdateSystemThemeRequest(
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor,
    string SurfaceColor,
    string TextColor,
    int BorderRadius);
