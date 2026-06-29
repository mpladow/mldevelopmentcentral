using MldevDashboard.Domain.Systems;

namespace MldevDashboard.Application.Systems;

public interface ISystemRepository
{
    Task<AvailableSystemResponse[]> ListAvailableSystemsAsync(
        Guid accountId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SystemDefinition>> ListSystemsAsync(CancellationToken cancellationToken);

    Task<SystemDefinition?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<bool> ExistsByKeyAsync(
        string systemKey,
        CancellationToken cancellationToken);

    Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken);

    Task AddAsync(
        SystemDefinition systemDefinition,
        CancellationToken cancellationToken);

    void ReplaceAccountAccesses(
        SystemDefinition systemDefinition,
        Guid[] accountIds);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
