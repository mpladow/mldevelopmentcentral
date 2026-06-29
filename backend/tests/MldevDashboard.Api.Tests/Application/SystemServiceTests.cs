using MldevDashboard.Application.Common;
using MldevDashboard.Application.Systems;
using MldevDashboard.Domain.Systems;

namespace MldevDashboard.Api.Tests.Application;

public sealed class SystemServiceTests
{
    [Fact]
    public async Task CreateSystemAsync_CreatesSystemWithGeneratedKeyAndAccountAccess()
    {
        var accountId = Guid.NewGuid();
        var repository = new FakeSystemRepository();
        var accountLookup = new FakeSystemAccountLookup([
            new SystemAccountResponse(accountId, "user@example.com", "User Example")
        ]);
        var systemService = new SystemService(repository, accountLookup);

        var result = await systemService.CreateSystemAsync(
            new CreateSystemRequest("War Machine", [accountId]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("war-machine", result.Value.SystemKey);
        Assert.Single(result.Value.Accounts);
        Assert.Contains(repository.Systems, system => system.AccountAccesses.Any(access => access.AccountId == accountId));
    }

    [Fact]
    public async Task CreateSystemAsync_ReturnsBadRequestWhenAssignedAccountDoesNotExist()
    {
        var systemService = new SystemService(
            new FakeSystemRepository(),
            new FakeSystemAccountLookup([]));

        var result = await systemService.CreateSystemAsync(
            new CreateSystemRequest("War Machine", [Guid.NewGuid()]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.BadRequest, result.Status);
        Assert.Equal("One or more assigned accounts do not exist.", result.Message);
    }

    [Fact]
    public async Task CreateSystemAsync_ReturnsConflictWhenGeneratedKeyAlreadyExists()
    {
        var accountId = Guid.NewGuid();
        var repository = new FakeSystemRepository();
        repository.Systems.Add(new SystemDefinition { SystemKey = "war-machine", Label = "War Machine" });
        var accountLookup = new FakeSystemAccountLookup([
            new SystemAccountResponse(accountId, "user@example.com", "User Example")
        ]);
        var systemService = new SystemService(repository, accountLookup);

        var result = await systemService.CreateSystemAsync(
            new CreateSystemRequest("War Machine", [accountId]),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Conflict, result.Status);
        Assert.Equal("A system already exists with this name.", result.Message);
    }

    private sealed class FakeSystemRepository : ISystemRepository
    {
        public List<SystemDefinition> Systems { get; } = [];

        public Task<AvailableSystemResponse[]> ListAvailableSystemsAsync(
            Guid accountId,
            CancellationToken cancellationToken)
        {
            var systems = Systems
                .Where(system => system.IsActive)
                .Where(system => system.AccountAccesses.Any(access => access.AccountId == accountId))
                .Select(system => new AvailableSystemResponse(system.Id, system.SystemKey, system.Label))
                .ToArray();

            return Task.FromResult(systems);
        }

        public Task<IReadOnlyCollection<SystemDefinition>> ListSystemsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<SystemDefinition>>(Systems);
        }

        public Task<SystemDefinition?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Systems.SingleOrDefault(system => system.Id == id));
        }

        public Task<bool> ExistsByKeyAsync(
            string systemKey,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Systems.Any(system => system.SystemKey == systemKey));
        }

        public Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken)
        {
            var currentMax = Systems.Select(system => (int?)system.SortOrder).Max() ?? 0;
            return Task.FromResult(currentMax + 1);
        }

        public Task AddAsync(
            SystemDefinition systemDefinition,
            CancellationToken cancellationToken)
        {
            systemDefinition.Id = Systems.Count + 1;
            Systems.Add(systemDefinition);
            return Task.CompletedTask;
        }

        public void ReplaceAccountAccesses(
            SystemDefinition systemDefinition,
            Guid[] accountIds)
        {
            systemDefinition.AccountAccesses = accountIds
                .Select(accountId => new SystemAccountAccess
                {
                    SystemDefinitionId = systemDefinition.Id,
                    AccountId = accountId
                })
                .ToList();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSystemAccountLookup(SystemAccountResponse[] accounts) : ISystemAccountLookup
    {
        public Task<Guid[]> ListExistingAccountIdsAsync(
            Guid[] accountIds,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(accounts
                .Where(account => accountIds.Contains(account.Id))
                .Select(account => account.Id)
                .ToArray());
        }

        public Task<SystemAccountResponse[]> ListAccountsByIdAsync(
            Guid[] accountIds,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(accounts
                .Where(account => accountIds.Contains(account.Id))
                .ToArray());
        }
    }
}
