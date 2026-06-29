namespace MldevDashboard.Domain.Systems;

public sealed class SystemDefinition
{
    public int Id { get; set; }

    public string SystemKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
