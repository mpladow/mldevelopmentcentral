using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;
using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Global.Systems.CreateSystem;

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
            .Select(account => new SystemAccountAssignmentRequest(account.AccountId, account.Role.Trim()))
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

        var defaultRoleNames = CreateDefaultSystemRoles(label)
            .Select(role => role.Name)
            .ToArray();
        if (accountAssignments.Any(account =>
            !defaultRoleNames.Contains(account.Role, StringComparer.OrdinalIgnoreCase)))
        {
            return ApplicationResult<SystemResponse>.BadRequest("One or more assigned account roles are invalid.");
        }

        var currentMaxSortOrder = await dbContext.Systems
            .Select(system => (int?)system.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var systemDefinition = new SystemDefinition
        {
            Label = label,
            SystemKey = systemKey,
            SortOrder = currentMaxSortOrder + 1,
        };

        foreach (var role in CreateDefaultSystemRoles(label))
        {
            systemDefinition.Roles.Add(role);
        }
        dbContext.Systems.Add(systemDefinition);
        await dbContext.SaveChangesAsync(cancellationToken);

        var rolesByName = await dbContext.ApplicationRoles
            .Where(role => role.SystemDefinitionId == systemDefinition.Id)
            .ToDictionaryAsync(role => role.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);
        foreach (var account in accountAssignments)
        {
            if (!rolesByName.TryGetValue(account.Role, out var role))
            {
                return ApplicationResult<SystemResponse>.BadRequest("One or more assigned account roles are invalid.");
            }

            systemDefinition.AccountRoles.Add(new SystemAccountRole
            {
                SystemDefinitionId = systemDefinition.Id,
                AccountId = account.AccountId,
                ApplicationRole = role
            });
        }

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

    private static ApplicationRole[] CreateDefaultSystemRoles(string systemLabel)
    {
        var roleNames = new[]
        {
            ("Admin", 0),
            ("User", 1),
            ("Viewer", 2)
        };

        return roleNames
            .Select(role =>
            {
                var roleName = ApplicationRoleNames.CreateSystemRoleName(systemLabel, role.Item1);
                return new ApplicationRole
                {
                    Name = roleName,
                    DisplayName = roleName,
                    SortOrder = role.Item2
                };
            })
            .ToArray();
    }
}
