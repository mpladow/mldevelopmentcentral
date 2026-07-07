namespace MldevDashboard.Api.Messaging
{
    public interface IPasswordResetEmailPublisher
    {
        Task PublishAsync(
            PasswordResetEmailRequestedMessage message, CancellationToken cancellationToken);
    }
}
