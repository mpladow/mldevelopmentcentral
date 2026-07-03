using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Global.Systems.UpdateSystemTheme;

public sealed class UpdateSystemThemeHandler(MldevDashboardDbContext dbContext)
{
    public async Task<ApplicationResult<SystemThemeResponse>> HandleAsync(
        int id,
        UpdateSystemThemeRequest request,
        CancellationToken cancellationToken)
    {
        var system = await dbContext.Systems
            .Include(system => system.ThemeSettings)
            .SingleOrDefaultAsync(system => system.Id == id, cancellationToken);
        if (system is null)
        {
            return ApplicationResult<SystemThemeResponse>.NotFound("System not found.");
        }

        var theme = system.ThemeSettings ?? new SystemThemeSettings
        {
            SystemDefinitionId = system.Id
        };

        theme.PrimaryColor = request.PrimaryColor.Trim();
        theme.SecondaryColor = request.SecondaryColor.Trim();
        theme.BackgroundColor = request.BackgroundColor.Trim();
        theme.SurfaceColor = request.SurfaceColor.Trim();
        theme.TextColor = request.TextColor.Trim();
        theme.BorderRadius = request.BorderRadius;

        if (system.ThemeSettings is null)
        {
            system.ThemeSettings = theme;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApplicationResult<SystemThemeResponse>.Success(
            SystemResponseMapper.ToThemeResponse(theme));
    }
}
