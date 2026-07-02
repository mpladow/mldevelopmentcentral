namespace MldevDashboard.Api.Features.Systems.CreateSystem;

public sealed record CreateSystemRequest(string Label, SystemAccountAssignmentRequest[] Accounts);

public sealed record SystemAccountAssignmentRequest(Guid AccountId, string Role);
