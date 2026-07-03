using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Api.Features.Global.Systems.ListSystems;

public sealed class ListSystemsHandler(
    MldevDashboardDbContext dbContext,
    UserManager<ApplicationUser> userManager)
{
    public async Task<SystemResponse[]> HandleAsync(CancellationToken cancellationToken)
    {
        var systems = await dbContext.Systems
            .AsNoTracking()
            .Include(system => system.ThemeSettings)
            .Include(system => system.AccountRoles)
                .ThenInclude(accountRole => accountRole.ApplicationRole)
            .OrderBy(system => system.SortOrder)
            .ToArrayAsync(cancellationToken);

        return await SystemResponseMapper.ToResponsesAsync(systems, userManager, cancellationToken);
    }
}
