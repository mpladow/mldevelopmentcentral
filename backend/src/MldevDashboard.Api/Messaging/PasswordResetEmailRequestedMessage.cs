namespace MldevDashboard.Api.Messaging
{
    public sealed record PasswordResetEmailRequestedMessage(
        string SchemaVersion,
        string CorrelationId,
        string Email,
        string ResetUrl,
        DateTimeOffset RequestedAtUtc);
}
