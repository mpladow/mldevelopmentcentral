using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Application.Accounts;
using MldevDashboard.Application.Common;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Infrastructure.Accounts;

public sealed class IdentityAccountService(UserManager<ApplicationUser> userManager) : IAccountService
{
    public async Task<AccountResponse[]> ListAccountsAsync(CancellationToken cancellationToken)
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

    public async Task<ApplicationResult<AccountResponse>> CreateAccountAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var roleValidation = ValidateRoles(request.Roles);
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
            return ApplicationResult<AccountResponse>.BadRequest(ToErrorDescriptions(createResult.Errors));
        }

        var addRolesResult = await userManager.AddToRolesAsync(user, roleValidation.RequestedRoles);
        if (!addRolesResult.Succeeded)
        {
            return ApplicationResult<AccountResponse>.BadRequest(ToErrorDescriptions(addRolesResult.Errors));
        }

        return ApplicationResult<AccountResponse>.Created(
            new AccountResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, roleValidation.RequestedRoles));
    }

    public async Task<ApplicationResult<AccountResponse>> UpdateAccountAsync(
        Guid id,
        UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var roleValidation = ValidateRoles(request.Roles);
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
            return ApplicationResult<AccountResponse>.BadRequest(ToErrorDescriptions(updateResult.Errors));
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
                return ApplicationResult<AccountResponse>.BadRequest(ToErrorDescriptions(removeRolesResult.Errors));
            }
        }

        if (rolesToAdd.Length > 0)
        {
            var addRolesResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addRolesResult.Succeeded)
            {
                return ApplicationResult<AccountResponse>.BadRequest(ToErrorDescriptions(addRolesResult.Errors));
            }
        }

        return ApplicationResult<AccountResponse>.Success(
            new AccountResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, roleValidation.RequestedRoles));
    }

    private static RoleValidationResult ValidateRoles(string[] roles)
    {
        var requestedRoles = roles.Length == 0
            ? [ApplicationRoles.User]
            : roles;

        var invalidRoles = requestedRoles
            .Except(ApplicationRoles.All, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new RoleValidationResult(requestedRoles, invalidRoles);
    }

    private static string[] ToErrorDescriptions(IEnumerable<IdentityError> errors)
    {
        return errors.Select(error => error.Description).ToArray();
    }

    private sealed record RoleValidationResult(string[] RequestedRoles, string[] InvalidRoles);
}
