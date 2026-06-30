using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MldevDashboard.Api.Common;
using MldevDashboard.Infrastructure.Identity;
using MldevDashboard.Infrastructure.Persistence;
using MldevDashboard.Infrastructure.Systems;

namespace MldevDashboard.Api.Features.Systems;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/systems")
            .WithTags("Systems")
            .RequireAuthorization();

        group.MapGet("/available", ListAvailableSystemsAsync);

        var adminGroup = group.MapGroup("")
            .RequireAuthorization(policy => policy.RequireRole(ApplicationRoles.Admin));

        adminGroup.MapGet("/", ListSystemsAsync);
        adminGroup.MapPost("/", CreateSystemAsync);
        adminGroup.MapPut("/{id:int}", UpdateSystemAsync);

        return app;
    }

    public static async Task<IResult> ListAvailableSystemsAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        MldevDashboardDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userIdValue = userManager.GetUserId(principal);
        if (!Guid.TryParse(userIdValue, out var accountId))
        {
            return Results.Unauthorized();
        }

        var systems = await dbContext.Systems
            .AsNoTracking()
            .Where(system => system.IsActive)
            .Where(system => system.AccountAccesses.Any(access => access.AccountId == accountId))
            .OrderBy(system => system.SortOrder)
            .Select(system => new AvailableSystemResponse(system.Id, system.SystemKey, system.Label))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(systems);
    }

    public static async Task<IResult> ListSystemsAsync(
        MldevDashboardDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await ListSystemResponsesAsync(dbContext, userManager, cancellationToken));
    }

    public static async Task<IResult> CreateSystemAsync(
        CreateSystemRequest request,
        MldevDashboardDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var result = await CreateSystemResultAsync(request, dbContext, userManager, cancellationToken);
        return result.ToHttpResult(system => $"/api/systems/{system.Id}");
    }

    public static async Task<IResult> UpdateSystemAsync(
        int id,
        UpdateSystemRequest request,
        MldevDashboardDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var result = await UpdateSystemResultAsync(id, request, dbContext, userManager, cancellationToken);
        return result.ToHttpResult();
    }

    internal static async Task<ApplicationResult<SystemResponse>> CreateSystemResultAsync(
        CreateSystemRequest request,
        MldevDashboardDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var label = request.Label.Trim();
        if (string.IsNullOrWhiteSpace(label))
        {
            return ApplicationResult<SystemResponse>.BadRequest("System name is required.");
        }

        var accountIds = request.AccountIds.Distinct().ToArray();
        if (!await AllAccountsExistAsync(accountIds, userManager, cancellationToken))
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
            AccountAccesses = accountIds
                .Select(accountId => new SystemAccountAccess { AccountId = accountId })
                .ToList()
        };

        dbContext.Systems.Add(systemDefinition);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = (await ToResponsesAsync([systemDefinition], userManager, cancellationToken)).Single();
        return ApplicationResult<SystemResponse>.Created(response);
    }

    internal static async Task<ApplicationResult<SystemResponse>> UpdateSystemResultAsync(
        int id,
        UpdateSystemRequest request,
        MldevDashboardDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var label = request.Label.Trim();
        if (string.IsNullOrWhiteSpace(label))
        {
            return ApplicationResult<SystemResponse>.BadRequest("System name is required.");
        }

        var accountIds = request.AccountIds.Distinct().ToArray();
        if (!await AllAccountsExistAsync(accountIds, userManager, cancellationToken))
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
        foreach (var accountId in accountIds)
        {
            systemDefinition.AccountAccesses.Add(new SystemAccountAccess
            {
                SystemDefinitionId = systemDefinition.Id,
                AccountId = accountId
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = (await ToResponsesAsync([systemDefinition], userManager, cancellationToken)).Single();
        return ApplicationResult<SystemResponse>.Success(response);
    }

    private static async Task<SystemResponse[]> ListSystemResponsesAsync(
        MldevDashboardDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var systems = await dbContext.Systems
            .AsNoTracking()
            .Include(system => system.AccountAccesses)
            .OrderBy(system => system.SortOrder)
            .ToArrayAsync(cancellationToken);

        return await ToResponsesAsync(systems, userManager, cancellationToken);
    }

    private static async Task<SystemResponse[]> ToResponsesAsync(
        IReadOnlyCollection<SystemDefinition> systems,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var accountIds = systems
            .SelectMany(system => system.AccountAccesses.Select(access => access.AccountId))
            .Distinct()
            .ToArray();

        var accounts = await userManager.Users
            .Where(user => accountIds.Contains(user.Id))
            .Select(user => new SystemAccountResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName))
            .ToArrayAsync(cancellationToken);
        var accountsById = accounts.ToDictionary(account => account.Id);

        return systems
            .OrderBy(system => system.SortOrder)
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

    private static async Task<bool> AllAccountsExistAsync(
        Guid[] accountIds,
        UserManager<ApplicationUser> userManager,
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
}

public sealed record CreateSystemRequest(string Label, Guid[] AccountIds);

public sealed record UpdateSystemRequest(string Label, Guid[] AccountIds);

public sealed record SystemResponse(
    int Id,
    string SystemKey,
    string Label,
    int SortOrder,
    bool IsActive,
    SystemAccountResponse[] Accounts);

public sealed record SystemAccountResponse(
    Guid Id,
    string Email,
    string DisplayName);

public sealed record AvailableSystemResponse(
    int Id,
    string SystemKey,
    string Label);
