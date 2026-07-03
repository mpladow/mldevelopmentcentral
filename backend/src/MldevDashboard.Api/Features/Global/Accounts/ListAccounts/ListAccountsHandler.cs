using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Features.Global.Accounts;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;

namespace MldevDashboard.Api.Features.Global.Accounts.ListAccounts;

public sealed class ListAccountsHandler(
    UserManager<ApplicationUser> userManager,
    MldevDashboardDbContext dbContext)
{
    public async Task<AccountResponse[]> HandleAsync(CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .OrderBy(user => user.Email)
            .Select(user => new AccountResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                Array.Empty<string>()))
            .ToListAsync(cancellationToken);

        var userIds = users.Select(user => user.Id).ToArray();
        var rolesByAccountId = await dbContext.SystemAccountRoles
            .Where(accountRole => userIds.Contains(accountRole.AccountId))
            .GroupBy(accountRole => accountRole.AccountId)
            .Select(group => new
            {
                AccountId = group.Key,
                Roles = group
                    .Select(accountRole => accountRole.ApplicationRole.Name)
                    .OrderBy(role => role)
                    .ToArray()
            })
            .ToDictionaryAsync(group => group.AccountId, group => group.Roles, cancellationToken);

        return users
            .Select(user => user with
            {
                Roles = rolesByAccountId.GetValueOrDefault(user.Id) ?? Array.Empty<string>()
            })
            .ToArray();
    }
}
