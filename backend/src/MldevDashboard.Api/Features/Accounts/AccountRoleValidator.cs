using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Accounts;

internal static class AccountRoleValidator
{
    public static RoleValidationResult ValidateRoles(string[] roles)
    {
        var requestedRoles = roles.Length == 0
            ? [ApplicationRoles.User]
            : roles;

        var invalidRoles = requestedRoles
            .Except(ApplicationRoles.All, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new RoleValidationResult(requestedRoles, invalidRoles);
    }
}

internal sealed record RoleValidationResult(string[] RequestedRoles, string[] InvalidRoles);
