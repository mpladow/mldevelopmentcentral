using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Application.Systems;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Infrastructure.Systems;

public sealed class IdentitySystemAccountLookup(UserManager<ApplicationUser> userManager) : ISystemAccountLookup
{
    public async Task<Guid[]> ListExistingAccountIdsAsync(
        Guid[] accountIds,
        CancellationToken cancellationToken)
    {
        return await userManager.Users
            .Where(user => accountIds.Contains(user.Id))
            .Select(user => user.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<SystemAccountResponse[]> ListAccountsByIdAsync(
        Guid[] accountIds,
        CancellationToken cancellationToken)
    {
        return await userManager.Users
            .Where(user => accountIds.Contains(user.Id))
            .Select(user => new SystemAccountResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName))
            .ToArrayAsync(cancellationToken);
    }
}
