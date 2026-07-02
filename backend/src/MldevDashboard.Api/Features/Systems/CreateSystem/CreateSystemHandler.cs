using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Systems.CreateSystem;

public sealed class CreateSystemHandler(
    MldevDashboardDbContext dbContext,
    UserManager<ApplicationUser> userManager)
{
    public async Task<ApplicationResult<SystemResponse>> HandleAsync(
        CreateSystemRequest request,
        CancellationToken cancellationToken)
    {
        var label = request.Label.Trim();
        var accountAssignments = (request.Accounts ?? [])
            .Select(account => new SystemAccountAssignmentRequest(account.AccountId, NormalizeRole(account.Role)))
            .DistinctBy(account => account.AccountId)
            .ToArray();
        if (accountAssignments.Any(account => string.IsNullOrWhiteSpace(account.Role)))
        {
            return ApplicationResult<SystemResponse>.BadRequest("One or more assigned account roles are invalid.");
        }

        var accountIds = accountAssignments.Select(account => account.AccountId).ToArray();
        if (!await AllAccountsExistAsync(accountIds, cancellationToken))
        {
            return ApplicationResult<SystemResponse>.BadRequest("One or more assigned accounts do not exist.");
        }

        var systemKey = CreateSystemKey(label);
        if (await dbContext.Systems.AnyAsync(system => system.SystemKey == systemKey, cancellationToken))
        {
            return ApplicationResult<SystemResponse>.Conflict("A system already exists with this name.");
        }

        var currentMaxSortOrder = await dbContext.Systems
            .Select(system => (int?)system.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var systemDefinition = new SystemDefinition
        {
            Label = label,
            SystemKey = systemKey,
            SortOrder = currentMaxSortOrder + 1,
            AccountAccesses = accountAssignments
                .Select(account => new SystemAccountAccess
                {
                    AccountId = account.AccountId,
                    Role = account.Role
                })
                .ToList()
        };

        dbContext.Systems.Add(systemDefinition);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = (await SystemResponseMapper.ToResponsesAsync([systemDefinition], userManager, cancellationToken)).Single();
        return ApplicationResult<SystemResponse>.Created(response);
    }

    private async Task<bool> AllAccountsExistAsync(
        Guid[] accountIds,
        CancellationToken cancellationToken)
    {
        var existingAccountIds = await userManager.Users
            .Where(user => accountIds.Contains(user.Id))
            .Select(user => user.Id)
            .ToArrayAsync(cancellationToken);

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

    private static string NormalizeRole(string role)
    {
        var requestedRole = role?.Trim() ?? string.Empty;

        return ApplicationRoles.All.FirstOrDefault(
            validRole => string.Equals(validRole, requestedRole, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }
}
