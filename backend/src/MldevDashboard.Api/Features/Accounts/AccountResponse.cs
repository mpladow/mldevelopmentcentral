namespace MldevDashboard.Api.Features.Accounts;

public sealed record AccountResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string[] Roles);
