using Microsoft.AspNetCore.Identity;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Auth;
using MldevDashboard.Api.Identity;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Api.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserHandler(
    UserManager<ApplicationUser> userManager,
    AppAuthorizationService appAuthorizationService)
{
    public async Task<ApplicationResult<CurrentUserResponse>> HandleAsync(GetCurrentUserRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return ApplicationResult<CurrentUserResponse>.Unauthorized();
        }

        var access = await appAuthorizationService.GetAccessAsync(user.Id);
        return ApplicationResult<CurrentUserResponse>.Success(
            new CurrentUserResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                access.Roles,
                access.Permissions));
    }
}
