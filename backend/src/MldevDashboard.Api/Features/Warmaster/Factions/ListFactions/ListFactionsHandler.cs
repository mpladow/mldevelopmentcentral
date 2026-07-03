namespace MldevDashboard.Api.Features.Warmaster.Factions.ListFactions;

public sealed class ListFactionsHandler
{
    public Task<FactionResponse[]> HandleAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Array.Empty<FactionResponse>());
    }
}
