using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Global.Systems.UpdateSystem;

public sealed class UpdateSystemHandler(
    MldevDashboardDbContext dbContext,
    UserManager<ApplicationUser> userManager)
{
    public async Task<ApplicationResult<SystemResponse>> HandleAsync(
        int id,
        UpdateSystemRequest request,
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

        var systemDefinition = await dbContext.Systems
            .Include(system => system.Roles)
            .Include(system => system.AccountRoles)
            .SingleOrDefaultAsync(system => system.Id == id, cancellationToken);
        if (systemDefinition is null)
        {
            return ApplicationResult<SystemResponse>.NotFound("System not found.");
        }

        systemDefinition.Label = label;
        EnsureDefaultSystemRoles(systemDefinition);
        var rolesByName = systemDefinition.Roles.ToDictionary(role => role.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var account in accountAssignments)
        {
            if (!rolesByName.ContainsKey(account.Role))
            {
                return ApplicationResult<SystemResponse>.BadRequest("One or more assigned account roles are invalid.");
            }
        }

        systemDefinition.AccountRoles.Clear();
        foreach (var account in accountAssignments)
        {
            systemDefinition.AccountRoles.Add(new SystemAccountRole
            {
                SystemDefinitionId = systemDefinition.Id,
                AccountId = account.AccountId,
                ApplicationRole = rolesByName[account.Role]
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = (await SystemResponseMapper.ToResponsesAsync([systemDefinition], userManager, cancellationToken)).Single();
        return ApplicationResult<SystemResponse>.Success(response);
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

    private static void EnsureDefaultSystemRoles(SystemDefinition systemDefinition)
    {
        var roleNames = new[]
        {
            ("Admin", 0),
            ("User", 1),
            ("Viewer", 2)
        };

        foreach (var (suffix, sortOrder) in roleNames)
        {
            var roleName = ApplicationRoleNames.CreateSystemRoleName(systemDefinition.Label, suffix);
            if (systemDefinition.Roles.Any(role => role.Name == roleName))
            {
                continue;
            }

            systemDefinition.Roles.Add(new ApplicationRole
            {
                SystemDefinitionId = systemDefinition.Id,
                Name = roleName,
                DisplayName = roleName,
                SortOrder = sortOrder
            });
        }
    }
}
