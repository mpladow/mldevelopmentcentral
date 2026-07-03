namespace MldevDashboard.Api.Features.Global.Accounts.UpdateAccount;

public sealed record UpdateAccountRequest(
    string Email,
    string DisplayName,
    string[] Roles);
