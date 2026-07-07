
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MldevDashboard.Api.Messaging
{
    public sealed class ServiceBusPasswordResetEmailPublisher : IPasswordResetEmailPublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public ServiceBusPasswordResetEmailPublisher(IOptions<ServiceBusOptions> options)
        {
            var serviceBus = options.Value;

            if (string.IsNullOrWhiteSpace(serviceBus.PasswordResetQueueName))
            {
                throw new InvalidOperationException("Service Bus password reset queue name is required.");
            }

            if (string.IsNullOrWhiteSpace(serviceBus.ConnectionString)
                && string.IsNullOrWhiteSpace(serviceBus.FullyQualifiedNamespace))
            {
                throw new InvalidOperationException(
                    "Service Bus connection string or fully qualified namespace is required.");
            }

            _client = !string.IsNullOrWhiteSpace(serviceBus.ConnectionString)
                ? new ServiceBusClient(serviceBus.ConnectionString)
                : new ServiceBusClient(serviceBus.FullyQualifiedNamespace, new DefaultAzureCredential());
            _sender = _client.CreateSender(serviceBus.PasswordResetQueueName);
        }
        public async Task PublishAsync(PasswordResetEmailRequestedMessage message, CancellationToken cancellationToken)
        {
            var body = JsonSerializer.Serialize(message);

            var serviceBusMessage = new ServiceBusMessage(body)
            {
                ContentType = "application/json",
                CorrelationId = message.CorrelationId.ToString(),
                MessageId = message.CorrelationId,
                Subject = "password-reset-email-requested",
            };
            serviceBusMessage.ApplicationProperties["schemaVersion"] = message.SchemaVersion.ToString();

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
        }
        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
