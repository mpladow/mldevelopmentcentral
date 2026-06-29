using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MldevDashboard.Application.Common;
using MldevDashboard.Application.Systems;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Api.Endpoints;

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

    private static async Task<IResult> ListAvailableSystemsAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ISystemService systemService,
        CancellationToken cancellationToken)
    {
        var userIdValue = userManager.GetUserId(principal);
        if (!Guid.TryParse(userIdValue, out var accountId))
        {
            return Results.Unauthorized();
        }

        var result = await systemService.ListAvailableSystemsAsync(accountId, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ListSystemsAsync(
        ISystemService systemService,
        CancellationToken cancellationToken)
    {
        return Results.Ok(await systemService.ListSystemsAsync(cancellationToken));
    }

    private static async Task<IResult> CreateSystemAsync(
        CreateSystemRequest request,
        ISystemService systemService,
        CancellationToken cancellationToken)
    {
        var result = await systemService.CreateSystemAsync(request, cancellationToken);
        return result.ToHttpResult(system => $"/api/systems/{system.Id}");
    }

    private static async Task<IResult> UpdateSystemAsync(
        int id,
        UpdateSystemRequest request,
        ISystemService systemService,
        CancellationToken cancellationToken)
    {
        var result = await systemService.UpdateSystemAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }
}
