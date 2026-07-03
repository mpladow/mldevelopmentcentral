namespace MldevDashboard.Api.Features.Warmaster.Units.ListUnits;

public sealed class ListUnitsHandler
{
    public Task<UnitResponse[]> HandleAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Array.Empty<UnitResponse>());
    }
}
