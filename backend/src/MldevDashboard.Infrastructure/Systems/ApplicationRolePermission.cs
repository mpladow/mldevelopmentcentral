namespace MldevDashboard.Infrastructure.Systems;

public sealed class ApplicationRolePermission
{
    public int ApplicationRoleId { get; set; }

    public int ApplicationPermissionId { get; set; }

    public ApplicationRole ApplicationRole { get; set; } = null!;

    public ApplicationPermission ApplicationPermission { get; set; } = null!;
}
