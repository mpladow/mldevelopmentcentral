using Microsoft.EntityFrameworkCore;
using MldevDashboard.Application.Systems;
using MldevDashboard.Domain.Systems;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Infrastructure.Systems;

public sealed class EfSystemRepository(MldevDashboardDbContext dbContext) : ISystemRepository
{
    public async Task<AvailableSystemResponse[]> ListAvailableSystemsAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Systems
            .Where(system => system.IsActive)
            .Where(system => system.AccountAccesses.Any(access => access.AccountId == accountId))
            .OrderBy(system => system.SortOrder)
            .ThenBy(system => system.Label)
            .Select(system => new AvailableSystemResponse(
                system.Id,
                system.SystemKey,
                system.Label))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SystemDefinition>> ListSystemsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Systems
            .Include(system => system.AccountAccesses)
            .OrderBy(system => system.SortOrder)
            .ThenBy(system => system.Label)
            .ToListAsync(cancellationToken);
    }

    public async Task<SystemDefinition?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await dbContext.Systems
            .Include(system => system.AccountAccesses)
            .SingleOrDefaultAsync(system => system.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByKeyAsync(
        string systemKey,
        CancellationToken cancellationToken)
    {
        return await dbContext.Systems
            .AnyAsync(system => system.SystemKey == systemKey, cancellationToken);
    }

    public async Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken)
    {
        var currentMax = await dbContext.Systems
            .Select(system => (int?)system.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        return currentMax + 1;
    }

    public async Task AddAsync(
        SystemDefinition systemDefinition,
        CancellationToken cancellationToken)
    {
        await dbContext.Systems.AddAsync(systemDefinition, cancellationToken);
    }

    public void ReplaceAccountAccesses(
        SystemDefinition systemDefinition,
        Guid[] accountIds)
    {
        dbContext.SystemAccountAccesses.RemoveRange(systemDefinition.AccountAccesses);
        systemDefinition.AccountAccesses = accountIds
            .Select(accountId => new SystemAccountAccess
            {
                SystemDefinitionId = systemDefinition.Id,
                AccountId = accountId
            })
            .ToList();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
