using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Systems;

internal static class SystemResponseMapper
{
    public static async Task<SystemResponse[]> ToResponsesAsync(
        IReadOnlyCollection<SystemDefinition> systems,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var accountIds = systems
            .SelectMany(system => system.AccountAccesses.Select(access => access.AccountId))
            .Distinct()
            .ToArray();

        var accounts = await userManager.Users
            .Where(user => accountIds.Contains(user.Id))
            .Select(user => new
            {
                user.Id,
                Email = user.Email ?? string.Empty,
                user.DisplayName
            })
            .ToArrayAsync(cancellationToken);
        var accountsById = accounts.ToDictionary(account => account.Id);

        return systems
            .OrderBy(system => system.SortOrder)
            .Select(system => new SystemResponse(
                system.Id,
                system.SystemKey,
                system.Label,
                system.SortOrder,
                system.IsActive,
                ToThemeResponse(system.ThemeSettings),
                system.AccountAccesses
                    .Select(access =>
                    {
                        var account = accountsById.GetValueOrDefault(access.AccountId);
                        return account is null
                            ? null
                            : new SystemAccountResponse(account.Id, account.Email, account.DisplayName, access.Role);
                    })
                    .OfType<SystemAccountResponse>()
                    .OrderBy(account => account.DisplayName)
                    .ThenBy(account => account.Email)
                    .ToArray()))
            .ToArray();
    }

    public static SystemThemeResponse ToThemeResponse(SystemThemeSettings? theme)
    {
        return new SystemThemeResponse(
            theme?.PrimaryColor ?? "#1d4ed8",
            theme?.SecondaryColor ?? "#0f766e",
            theme?.BackgroundColor ?? "#f6f8fb",
            theme?.SurfaceColor ?? "#ffffff",
            theme?.TextColor ?? "#111827",
            theme?.BorderRadius ?? 8);
    }
}
