namespace MldevDashboard.Api.Features.Global.Systems.CreateSystem;

public sealed record CreateSystemRequest(string Label, SystemAccountAssignmentRequest[] Accounts);

public sealed record SystemAccountAssignmentRequest(Guid AccountId, string Role);
