using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Api.Features.Global.Systems.GetSystemTheme;

public sealed class GetSystemThemeHandler(MldevDashboardDbContext dbContext)
{
    public async Task<ApplicationResult<SystemThemeResponse>> HandleAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var system = await dbContext.Systems
            .AsNoTracking()
            .Include(system => system.ThemeSettings)
            .SingleOrDefaultAsync(system => system.Id == id, cancellationToken);
        if (system is null)
        {
            return ApplicationResult<SystemThemeResponse>.NotFound("System not found.");
        }

        return ApplicationResult<SystemThemeResponse>.Success(
            SystemResponseMapper.ToThemeResponse(system.ThemeSettings));
    }
}
