namespace MldevDashboard.Api.Features.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    CurrentUserResponse User);

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string[] Roles);
