using Microsoft.AspNetCore.Authorization;

namespace MldevDashboard.Api.Common;

public static class AuthorizationPolicyExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(
        this AuthorizationPolicyBuilder policy,
        string permission)
    {
        return policy
            .RequireAuthenticatedUser()
            .RequireClaim(AppClaimTypes.Permission, permission);
    }
}
