namespace MldevDashboard.Api.Features.Systems;

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
