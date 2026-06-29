using MldevDashboard.Application.Common;

namespace MldevDashboard.Application.Systems;

public interface ISystemService
{
    Task<ApplicationResult<AvailableSystemResponse[]>> ListAvailableSystemsAsync(
        Guid accountId,
        CancellationToken cancellationToken);

    Task<SystemResponse[]> ListSystemsAsync(CancellationToken cancellationToken);

    Task<ApplicationResult<SystemResponse>> CreateSystemAsync(
        CreateSystemRequest request,
        CancellationToken cancellationToken);

    Task<ApplicationResult<SystemResponse>> UpdateSystemAsync(
        int id,
        UpdateSystemRequest request,
        CancellationToken cancellationToken);
}

public sealed record CreateSystemRequest(string Label, Guid[] AccountIds);

public sealed record UpdateSystemRequest(string Label, Guid[] AccountIds);

public sealed record SystemResponse(
    int Id,
    string SystemKey,
    string Label,
    int SortOrder,
    bool IsActive,
    SystemAccountResponse[] Accounts);

public sealed record SystemAccountResponse(
    Guid Id,
    string Email,
    string DisplayName);

public sealed record AvailableSystemResponse(
    int Id,
    string SystemKey,
    string Label);
