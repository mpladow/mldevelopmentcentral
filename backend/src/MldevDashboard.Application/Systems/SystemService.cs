using MldevDashboard.Application.Common;
using MldevDashboard.Domain.Systems;

namespace MldevDashboard.Application.Systems;

public sealed class SystemService(
    ISystemRepository systemRepository,
    ISystemAccountLookup accountLookup) : ISystemService
{
    public async Task<ApplicationResult<AvailableSystemResponse[]>> ListAvailableSystemsAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        return ApplicationResult<AvailableSystemResponse[]>.Success(
            await systemRepository.ListAvailableSystemsAsync(accountId, cancellationToken));
    }

    public async Task<SystemResponse[]> ListSystemsAsync(CancellationToken cancellationToken)
    {
        var systems = await systemRepository.ListSystemsAsync(cancellationToken);
        return await ToResponsesAsync(systems, cancellationToken);
    }

    public async Task<ApplicationResult<SystemResponse>> CreateSystemAsync(
        CreateSystemRequest request,
        CancellationToken cancellationToken)
    {
        var label = request.Label.Trim();
        if (string.IsNullOrWhiteSpace(label))
        {
            return ApplicationResult<SystemResponse>.BadRequest("System name is required.");
        }

        var accountIds = request.AccountIds.Distinct().ToArray();
        if (!await AllAccountsExistAsync(accountIds, cancellationToken))
        {
            return ApplicationResult<SystemResponse>.BadRequest("One or more assigned accounts do not exist.");
        }

        var systemKey = CreateSystemKey(label);
        if (await systemRepository.ExistsByKeyAsync(systemKey, cancellationToken))
        {
            return ApplicationResult<SystemResponse>.Conflict("A system already exists with this name.");
        }

        var systemDefinition = new SystemDefinition
        {
            Label = label,
            SystemKey = systemKey,
            SortOrder = await systemRepository.GetNextSortOrderAsync(cancellationToken),
            AccountAccesses = accountIds
                .Select(accountId => new SystemAccountAccess { AccountId = accountId })
                .ToList()
        };

        await systemRepository.AddAsync(systemDefinition, cancellationToken);
        await systemRepository.SaveChangesAsync(cancellationToken);

        var response = (await ToResponsesAsync([systemDefinition], cancellationToken)).Single();
        return ApplicationResult<SystemResponse>.Created(response);
    }

    public async Task<ApplicationResult<SystemResponse>> UpdateSystemAsync(
        int id,
        UpdateSystemRequest request,
        CancellationToken cancellationToken)
    {
        var label = request.Label.Trim();
        if (string.IsNullOrWhiteSpace(label))
        {
            return ApplicationResult<SystemResponse>.BadRequest("System name is required.");
        }

        var accountIds = request.AccountIds.Distinct().ToArray();
        if (!await AllAccountsExistAsync(accountIds, cancellationToken))
        {
            return ApplicationResult<SystemResponse>.BadRequest("One or more assigned accounts do not exist.");
        }

        var systemDefinition = await systemRepository.GetByIdAsync(id, cancellationToken);
        if (systemDefinition is null)
        {
            return ApplicationResult<SystemResponse>.NotFound("System not found.");
        }

        systemDefinition.Label = label;
        systemRepository.ReplaceAccountAccesses(systemDefinition, accountIds);

        await systemRepository.SaveChangesAsync(cancellationToken);

        var response = (await ToResponsesAsync([systemDefinition], cancellationToken)).Single();
        return ApplicationResult<SystemResponse>.Success(response);
    }

    private async Task<SystemResponse[]> ToResponsesAsync(
        IReadOnlyCollection<SystemDefinition> systems,
        CancellationToken cancellationToken)
    {
        var accountIds = systems
            .SelectMany(system => system.AccountAccesses.Select(access => access.AccountId))
            .Distinct()
            .ToArray();

        var accounts = await accountLookup.ListAccountsByIdAsync(accountIds, cancellationToken);
        var accountsById = accounts.ToDictionary(account => account.Id);

        return systems
            .Select(system => new SystemResponse(
                system.Id,
                system.SystemKey,
                system.Label,
                system.SortOrder,
                system.IsActive,
                system.AccountAccesses
                    .Select(access => accountsById.GetValueOrDefault(access.AccountId))
                    .OfType<SystemAccountResponse>()
                    .OrderBy(account => account.DisplayName)
                    .ThenBy(account => account.Email)
                    .ToArray()))
            .ToArray();
    }

    private async Task<bool> AllAccountsExistAsync(
        Guid[] accountIds,
        CancellationToken cancellationToken)
    {
        var existingAccountIds = await accountLookup.ListExistingAccountIdsAsync(accountIds, cancellationToken);
        return !accountIds.Except(existingAccountIds).Any();
    }

    private static string CreateSystemKey(string label)
    {
        var keyCharacters = label
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();

        var key = string.Join(
            '-',
            new string(keyCharacters).Split('-', StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(key) ? "system" : key;
    }
}
