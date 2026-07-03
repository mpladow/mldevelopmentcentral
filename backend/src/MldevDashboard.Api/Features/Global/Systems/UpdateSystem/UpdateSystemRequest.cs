namespace MldevDashboard.Api.Features.Global.Systems.UpdateSystem;

public sealed record UpdateSystemRequest(string Label, SystemAccountAssignmentRequest[] Accounts);

public sealed record SystemAccountAssignmentRequest(Guid AccountId, string Role);
