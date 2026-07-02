namespace MldevDashboard.Infrastructure.Systems;

public sealed class SystemThemeSettings
{
    public int SystemDefinitionId { get; set; }

    public string PrimaryColor { get; set; } = "#1d4ed8";

    public string SecondaryColor { get; set; } = "#0f766e";

    public string BackgroundColor { get; set; } = "#f6f8fb";

    public string SurfaceColor { get; set; } = "#ffffff";

    public string TextColor { get; set; } = "#111827";

    public int BorderRadius { get; set; } = 8;

    public SystemDefinition SystemDefinition { get; set; } = null!;
}
