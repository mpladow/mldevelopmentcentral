namespace MldevDashboard.Api.Features.Systems.UpdateSystem;

public sealed record UpdateSystemRequest(string Label, Guid[] AccountIds);
