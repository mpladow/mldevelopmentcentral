namespace MldevDashboard.Api.Features.Systems;

public sealed record SystemResponse(
    int Id,
    string SystemKey,
    string Label,
    int SortOrder,
    bool IsActive,
    SystemThemeResponse Theme,
    SystemAccountResponse[] Accounts);

public sealed record SystemAccountResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Role);

public sealed record AvailableSystemResponse(
    int Id,
    string SystemKey,
    string Label);

public sealed record SystemThemeResponse(
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor,
    string SurfaceColor,
    string TextColor,
    int BorderRadius);
