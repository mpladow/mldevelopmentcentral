using MldevDashboard.Application.Common;

namespace MldevDashboard.Application.Accounts;

public interface IAccountService
{
    Task<AccountResponse[]> ListAccountsAsync(CancellationToken cancellationToken);

    Task<ApplicationResult<AccountResponse>> CreateAccountAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken);

    Task<ApplicationResult<AccountResponse>> UpdateAccountAsync(
        Guid id,
        UpdateAccountRequest request,
        CancellationToken cancellationToken);
}

public sealed record CreateAccountRequest(
    string Email,
    string DisplayName,
    string Password,
    string[] Roles);

public sealed record UpdateAccountRequest(
    string Email,
    string DisplayName,
    string[] Roles);

public sealed record AccountResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string[] Roles);
