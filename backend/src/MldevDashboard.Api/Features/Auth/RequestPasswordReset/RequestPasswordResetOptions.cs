namespace MldevDashboard.Api.Features.Auth.RequestPasswordReset;

public sealed class RequestPasswordResetOptions
{
    public const string SectionName = "Authentication:PasswordReset";

    public string ResetPasswordUrl { get; set; } = "http://localhost:5173/reset-password";
}
