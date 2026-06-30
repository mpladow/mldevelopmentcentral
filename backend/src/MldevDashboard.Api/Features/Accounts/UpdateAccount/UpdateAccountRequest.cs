namespace MldevDashboard.Api.Features.Accounts.UpdateAccount;

public sealed record UpdateAccountRequest(
    string Email,
    string DisplayName,
    string[] Roles);
