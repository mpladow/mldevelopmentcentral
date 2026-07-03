namespace MldevDashboard.Infrastructure.Systems;

public sealed class SystemDefinition
{
    public int Id { get; set; }

    public string SystemKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public SystemThemeSettings? ThemeSettings { get; set; }

    public ICollection<SystemAccountAccess> AccountAccesses { get; set; } = [];

    public ICollection<ApplicationRole> Roles { get; set; } = [];

    public ICollection<ApplicationPermission> Permissions { get; set; } = [];

    public ICollection<SystemAccountRole> AccountRoles { get; set; } = [];
}
