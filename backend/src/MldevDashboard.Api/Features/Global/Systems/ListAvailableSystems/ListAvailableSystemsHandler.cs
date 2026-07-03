using Microsoft.EntityFrameworkCore;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Api.Features.Global.Systems.ListAvailableSystems;

public sealed class ListAvailableSystemsHandler(MldevDashboardDbContext dbContext)
{
    public async Task<AvailableSystemResponse[]> HandleAsync(
        ListAvailableSystemsRequest request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Systems
            .AsNoTracking()
            .Where(system => system.IsActive)
            .Where(system => system.AccountRoles.Any(accountRole => accountRole.AccountId == request.AccountId))
            .OrderBy(system => system.SortOrder)
            .Select(system => new AvailableSystemResponse(system.Id, system.SystemKey, system.Label))
            .ToArrayAsync(cancellationToken);
    }
}
