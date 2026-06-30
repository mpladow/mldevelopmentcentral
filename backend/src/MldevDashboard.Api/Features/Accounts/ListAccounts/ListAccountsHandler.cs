using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Features.Accounts;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Api.Features.Accounts.ListAccounts;

public sealed class ListAccountsHandler(UserManager<ApplicationUser> userManager)
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

        var responses = new List<AccountResponse>();
        foreach (var user in users)
        {
            var appUser = await userManager.FindByIdAsync(user.Id.ToString());
            var roles = appUser is null
                ? Array.Empty<string>()
                : (await userManager.GetRolesAsync(appUser)).ToArray();

            responses.Add(user with { Roles = roles });
        }

        return responses.ToArray();
    }
}
