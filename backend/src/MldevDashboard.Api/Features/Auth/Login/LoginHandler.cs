using Microsoft.AspNetCore.Identity;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Features.Auth;
using MldevDashboard.Api.Identity;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Api.Features.Auth.Login;

public sealed class LoginHandler(
    UserManager<ApplicationUser> userManager,
    JwtTokenService tokenService)
{
    public async Task<ApplicationResult<LoginResponse>> HandleAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return ApplicationResult<LoginResponse>.Unauthorized();
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return ApplicationResult<LoginResponse>.Unauthorized();
        }

        var token = await tokenService.CreateTokenAsync(user);

        return ApplicationResult<LoginResponse>.Success(new LoginResponse(
            token.AccessToken,
            token.ExpiresAt,
            new CurrentUserResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, token.Roles)));
    }
}
