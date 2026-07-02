using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Systems.UpdateSystem;

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

        var systemDefinition = await dbContext.Systems
            .Include(system => system.AccountAccesses)
            .SingleOrDefaultAsync(system => system.Id == id, cancellationToken);
        if (systemDefinition is null)
        {
            return ApplicationResult<SystemResponse>.NotFound("System not found.");
        }

        systemDefinition.Label = label;
        systemDefinition.AccountAccesses.Clear();
        foreach (var account in accountAssignments)
        {
            systemDefinition.AccountAccesses.Add(new SystemAccountAccess
            {
                SystemDefinitionId = systemDefinition.Id,
                AccountId = account.AccountId,
                Role = account.Role
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

    private static string NormalizeRole(string role)
    {
        var requestedRole = role?.Trim() ?? string.Empty;

        return ApplicationRoles.All.FirstOrDefault(
            validRole => string.Equals(validRole, requestedRole, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }
}
