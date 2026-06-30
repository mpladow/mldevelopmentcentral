namespace MldevDashboard.Infrastructure.Systems;

public sealed class SystemAccountAccess
{
    public int SystemDefinitionId { get; set; }

    public Guid AccountId { get; set; }

    public SystemDefinition SystemDefinition { get; set; } = null!;
}
