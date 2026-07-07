namespace MldevDashboard.Api.Messaging
{
    public sealed class ServiceBusOptions
    {
        public const string SectionName = "Messaging:ServiceBus";
        public string ConnectionString { get; set; } = string.Empty;
        public string FullyQualifiedNamespace { get; set; } = string.Empty;
        public string PasswordResetQueueName { get; set; } = "password-reset-email-requests";

    }
}
