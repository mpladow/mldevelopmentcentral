namespace MldevDashboard.Application.Systems;

public interface ISystemAccountLookup
{
    Task<Guid[]> ListExistingAccountIdsAsync(
        Guid[] accountIds,
        CancellationToken cancellationToken);

    Task<SystemAccountResponse[]> ListAccountsByIdAsync(
        Guid[] accountIds,
        CancellationToken cancellationToken);
}
