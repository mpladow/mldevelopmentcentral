namespace MldevDashboard.Api.Features.Systems.CreateSystem;

public sealed record CreateSystemRequest(string Label, Guid[] AccountIds);
