namespace MldevDashboard.Api.Features.Global.Accounts.CreateAccount;

public sealed record CreateAccountRequest(
    string Email,
    string DisplayName,
    string Password,
    string[] Roles);
