using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Api.Identity;

public sealed class JwtTokenService(
    IOptions<JwtOptions> options,
    AppAuthorizationService appAuthorizationService)
{
    private readonly JwtOptions _options = options.Value;

    public async Task<IssuedToken> CreateTokenAsync(ApplicationUser user)
    {
        var access = await appAuthorizationService.GetAccessAsync(user.Id);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.ExpiryMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.DisplayName)
        };

        claims.AddRange(access.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(access.Permissions.Select(permission => new Claim(AppClaimTypes.Permission, permission)));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new IssuedToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            access.Roles,
            access.Permissions);
    }
}

public sealed record IssuedToken(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string[] Roles,
    string[] Permissions);
