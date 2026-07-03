namespace MldevDashboard.Infrastructure.Systems;

public sealed class ApplicationPermission
{
    public int Id { get; set; }

    public int SystemDefinitionId { get; set; }

    public string PermissionKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public SystemDefinition SystemDefinition { get; set; } = null!;

    public ICollection<ApplicationRolePermission> RolePermissions { get; set; } = [];
}
