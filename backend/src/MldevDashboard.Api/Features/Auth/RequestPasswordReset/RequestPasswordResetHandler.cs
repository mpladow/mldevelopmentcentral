using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MldevDashboard.Api.Common;
using MldevDashboard.Api.Messaging;
using MldevDashboard.Infrastructure.Identity;

namespace MldevDashboard.Api.Features.Auth.RequestPasswordReset;

public sealed class RequestPasswordResetHandler(
    UserManager<ApplicationUser> userManager,
    IPasswordResetEmailPublisher publisher,
    IOptions<RequestPasswordResetOptions> options)
{
    private const string SuccessMessage = "If an account exists, reset instructions will be emailed to the provided address";

    public async Task<ApplicationResult<RequestPasswordResetResponse>> HandleAsync(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var response = new RequestPasswordResetResponse(SuccessMessage);
        var email = request.Email.Trim();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return ApplicationResult<RequestPasswordResetResponse>.Success(response);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = QueryHelpers.AddQueryString(
            options.Value.ResetPasswordUrl,
            new Dictionary<string, string?>
            {
                ["email"] = user.Email,
                ["token"] = encodedToken
            });

        await publisher.PublishAsync(
            new PasswordResetEmailRequestedMessage(
                "1",
                Guid.NewGuid().ToString("N"),
                user.Email,
                resetUrl,
                DateTimeOffset.UtcNow),
            cancellationToken);

        return ApplicationResult<RequestPasswordResetResponse>.Success(response);
    }
}
