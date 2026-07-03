using MldevDashboard.Api.Common;

namespace MldevDashboard.Api.Features.Global.Roles.CreatePermission;

public static class CreatePermissionEndpoint
{
    public static async Task<IResult> HandleAsync(
        CreatePermissionRequest request,
        CreatePermissionHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Length > 0)
        {
            return Results.BadRequest(new { errors });
        }

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult(permission => $"/api/roles/permissions/{permission.Id}");
    }

    private static string[] Validate(CreatePermissionRequest request)
    {
        var errors = new List<string>();

        if (request.SystemId <= 0)
        {
            errors.Add("System is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PermissionKey))
        {
            errors.Add("Permission key is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors.Add("Display name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            errors.Add("Category is required.");
        }

        return errors.ToArray();
    }
}
