namespace MldevDashboard.Infrastructure.Systems;

public sealed class ApplicationRole
{
    public int Id { get; set; }

    public int SystemDefinitionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsProtected { get; set; }

    public int SortOrder { get; set; }

    public SystemDefinition SystemDefinition { get; set; } = null!;

    public ICollection<ApplicationRolePermission> RolePermissions { get; set; } = [];

    public ICollection<SystemAccountRole> AccountRoles { get; set; } = [];
}
