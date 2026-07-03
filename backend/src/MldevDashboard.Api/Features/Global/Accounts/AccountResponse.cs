namespace MldevDashboard.Api.Features.Global.Accounts;

public sealed record AccountResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string[] Roles);
