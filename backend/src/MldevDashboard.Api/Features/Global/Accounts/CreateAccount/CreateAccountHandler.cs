using Microsoft.AspNetCore.Identity;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Global.Accounts;
using MldevDashboard.Api.Identity;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Global.Accounts.CreateAccount;

public sealed class CreateAccountHandler(
    UserManager<ApplicationUser> userManager,
    MldevDashboardDbContext dbContext,
    AppAuthorizationService appAuthorizationService)
{
    public async Task<ApplicationResult<AccountResponse>> HandleAsync(
        CreateAccountRequest request,
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

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return ApplicationResult<AccountResponse>.Conflict("An account already exists for this email address.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            DisplayName = request.DisplayName
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return ApplicationResult<AccountResponse>.BadRequest(
                IdentityErrorMapper.ToErrorDescriptions(createResult.Errors));
        }

        dbContext.SystemAccountRoles.AddRange(roles.Select(role => new SystemAccountRole
        {
            AccountId = user.Id,
            SystemDefinitionId = role.SystemDefinitionId,
            ApplicationRoleId = role.Id
        }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApplicationResult<AccountResponse>.Created(
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
