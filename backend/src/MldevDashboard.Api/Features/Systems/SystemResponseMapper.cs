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
            .Select(user => new SystemAccountResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName))
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
                system.AccountAccesses
                    .Select(access => accountsById.GetValueOrDefault(access.AccountId))
                    .OfType<SystemAccountResponse>()
                    .OrderBy(account => account.DisplayName)
                    .ThenBy(account => account.Email)
                    .ToArray()))
            .ToArray();
    }
}
