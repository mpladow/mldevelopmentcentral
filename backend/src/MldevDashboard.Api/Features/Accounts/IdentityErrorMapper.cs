using Microsoft.AspNetCore.Identity;

namespace MldevDashboard.Api.Features.Accounts;

internal static class IdentityErrorMapper
{
    public static string[] ToErrorDescriptions(IEnumerable<IdentityError> errors)
    {
        return errors.Select(error => error.Description).ToArray();
    }
}
