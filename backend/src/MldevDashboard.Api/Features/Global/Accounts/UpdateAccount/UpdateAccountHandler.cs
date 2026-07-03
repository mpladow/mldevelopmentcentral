using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Global.Accounts;
using MldevDashboard.Api.Identity;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Global.Accounts.UpdateAccount;

public sealed class UpdateAccountHandler(
    UserManager<ApplicationUser> userManager,
    MldevDashboardDbContext dbContext,
    AppAuthorizationService appAuthorizationService)
{
    public async Task<ApplicationResult<AccountResponse>> HandleAsync(
        Guid id,
        UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestedRoles = NormalizeRequestedRoles(request.Roles);
        var roles = await appAuthorizationService.GetRolesByNameAsync(requestedRoles, cancellationToken);
        var invalidRoles = requestedRoles
            .Except(roles.Select(role => role.Name), StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (invalidRoles.Length > 0)
        {
            return ApplicationResult<AccountResponse>.BadRequest(
                $"Unknown role(s): {string.Join(", ", invalidRoles)}");
        }

        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return ApplicationResult<AccountResponse>.NotFound("Account not found.");
        }

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null && existingUser.Id != id)
        {
            return ApplicationResult<AccountResponse>.Conflict("An account already exists for this email address.");
        }

        user.Email = request.Email;
        user.UserName = request.Email;
        user.DisplayName = request.DisplayName;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return ApplicationResult<AccountResponse>.BadRequest(
                IdentityErrorMapper.ToErrorDescriptions(updateResult.Errors));
        }

        var currentAccountRoles = await dbContext.SystemAccountRoles
            .Where(accountRole => accountRole.AccountId == user.Id)
            .ToArrayAsync(cancellationToken);
        dbContext.SystemAccountRoles.RemoveRange(currentAccountRoles);
        dbContext.SystemAccountRoles.AddRange(roles.Select(role => new SystemAccountRole
        {
            AccountId = user.Id,
            SystemDefinitionId = role.SystemDefinitionId,
            ApplicationRoleId = role.Id
        }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApplicationResult<AccountResponse>.Success(
            new AccountResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, roles.Select(role => role.Name).ToArray()));
    }

    private static string[] NormalizeRequestedRoles(string[] roles)
    {
        return roles
            .Select(role => role.Trim())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
