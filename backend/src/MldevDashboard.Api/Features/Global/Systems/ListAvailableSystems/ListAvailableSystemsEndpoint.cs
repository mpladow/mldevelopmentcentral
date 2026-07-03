using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Api.Features.Global.Systems.ListAvailableSystems;

public static class ListAvailableSystemsEndpoint
{
    public static async Task<IResult> HandleAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ListAvailableSystemsHandler handler,
        CancellationToken cancellationToken)
    {
        var userIdValue = userManager.GetUserId(principal);
        if (!Guid.TryParse(userIdValue, out var accountId))
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await handler.HandleAsync(new ListAvailableSystemsRequest(accountId), cancellationToken));
    }
}
