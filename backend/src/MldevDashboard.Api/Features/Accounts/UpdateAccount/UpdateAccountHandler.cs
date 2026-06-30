using Microsoft.AspNetCore.Identity;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Accounts;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Api.Features.Accounts.UpdateAccount;

public sealed class UpdateAccountHandler(UserManager<ApplicationUser> userManager)
{
    public async Task<ApplicationResult<AccountResponse>> HandleAsync(
        Guid id,
        UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roleValidation = AccountRoleValidator.ValidateRoles(request.Roles);
        if (roleValidation.InvalidRoles.Length > 0)
        {
            return ApplicationResult<AccountResponse>.BadRequest(
                $"Unknown role(s): {string.Join(", ", roleValidation.InvalidRoles)}");
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

        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles
            .Except(roleValidation.RequestedRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var rolesToAdd = roleValidation.RequestedRoles
            .Except(currentRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeRolesResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeRolesResult.Succeeded)
            {
                return ApplicationResult<AccountResponse>.BadRequest(
                    IdentityErrorMapper.ToErrorDescriptions(removeRolesResult.Errors));
            }
        }

        if (rolesToAdd.Length > 0)
        {
            var addRolesResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addRolesResult.Succeeded)
            {
                return ApplicationResult<AccountResponse>.BadRequest(
                    IdentityErrorMapper.ToErrorDescriptions(addRolesResult.Errors));
            }
        }

        return ApplicationResult<AccountResponse>.Success(
            new AccountResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, roleValidation.RequestedRoles));
    }
}
