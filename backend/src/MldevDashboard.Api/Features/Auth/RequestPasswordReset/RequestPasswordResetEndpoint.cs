using MldevDashboard.Api.Common;
using System.ComponentModel.DataAnnotations;

namespace MldevDashboard.Api.Features.Auth.RequestPasswordReset
{
    public static class RequestPasswordResetEndpoint
    {
        public static async Task<IResult> HandleAsync(RequestPasswordResetRequest request, RequestPasswordResetHandler handler, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Results.BadRequest(new { message = "Email is required" });

            }
            if (!new EmailAddressAttribute().IsValid(request.Email))
            {
                return Results.BadRequest(new { message = "Email must be valid." });
            }

            var result = await handler.HandleAsync(new RequestPasswordResetRequest(request.Email.Trim()), cancellationToken);

            return result.ToHttpResult();
        }
    }
}
