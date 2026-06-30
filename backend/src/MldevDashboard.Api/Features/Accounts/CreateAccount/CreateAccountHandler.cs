using Microsoft.AspNetCore.Identity;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Accounts;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Api.Features.Accounts.CreateAccount;

public sealed class CreateAccountHandler(UserManager<ApplicationUser> userManager)
{
    public async Task<ApplicationResult<AccountResponse>> HandleAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roleValidation = AccountRoleValidator.ValidateRoles(request.Roles);
        if (roleValidation.InvalidRoles.Length > 0)
        {
            return ApplicationResult<AccountResponse>.BadRequest(
                $"Unknown role(s): {string.Join(", ", roleValidation.InvalidRoles)}");
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

        var addRolesResult = await userManager.AddToRolesAsync(user, roleValidation.RequestedRoles);
        if (!addRolesResult.Succeeded)
        {
            return ApplicationResult<AccountResponse>.BadRequest(
                IdentityErrorMapper.ToErrorDescriptions(addRolesResult.Errors));
        }

        return ApplicationResult<AccountResponse>.Created(
            new AccountResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, roleValidation.RequestedRoles));
    }
}
