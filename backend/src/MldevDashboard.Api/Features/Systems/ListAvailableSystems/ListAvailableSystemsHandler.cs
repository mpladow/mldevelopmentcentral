using Microsoft.EntityFrameworkCore;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Api.Features.Systems.ListAvailableSystems;

public sealed class ListAvailableSystemsHandler(MldevDashboardDbContext dbContext)
{
    public async Task<AvailableSystemResponse[]> HandleAsync(
        ListAvailableSystemsRequest request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Systems
            .AsNoTracking()
            .Where(system => system.IsActive)
            .Where(system => system.AccountAccesses.Any(access => access.AccountId == request.AccountId))
            .OrderBy(system => system.SortOrder)
            .Select(system => new AvailableSystemResponse(system.Id, system.SystemKey, system.Label))
            .ToArrayAsync(cancellationToken);
    }
}
