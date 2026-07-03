namespace MldevDashboard.Infrastructure.Systems;

public sealed class SystemAccountRole
{
    public int SystemDefinitionId { get; set; }

    public Guid AccountId { get; set; }

    public int ApplicationRoleId { get; set; }

    public SystemDefinition SystemDefinition { get; set; } = null!;

    public ApplicationRole ApplicationRole { get; set; } = null!;
}
