using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Api.Features.Auth.GetCurrentUser;

public static class GetCurrentUserEndpoint
{
    public static async Task<IResult> HandleAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        GetCurrentUserHandler handler)
    {
        var userIdValue = userManager.GetUserId(principal);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await handler.HandleAsync(new GetCurrentUserRequest(userId));
        return result.ToHttpResult();
    }
}
