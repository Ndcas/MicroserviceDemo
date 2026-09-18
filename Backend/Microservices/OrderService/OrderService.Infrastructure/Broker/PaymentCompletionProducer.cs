using System.Text.Json;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Configuration;
using OrderService.Application.Dtos;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Constants;

namespace OrderService.Infrastructure.Broker;

internal class PaymentCompletionProducer : IPaymentCompletionProducer
{
    private IProducer<string> _producer;

    public PaymentCompletionProducer(IPulsarClient pulsarClient, IConfiguration configuration)
    {
        var topic = configuration[EnvironmentVariableKeys.PaymentCompletedTopic];

        _producer = pulsarClient.NewProducer(Schema.String).Topic(topic).Create();
    }

    public async Task SendAsync(
        Guid eventId,
        IReadOnlyList<ProductWithQuantityItem> message,
        CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonSerializer.Serialize(message);

        await _producer
            .NewMessage()
            .Property(BrokerEventConfigurations.EventIdProperyKey, eventId.ToString())
            .Send(jsonContent, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _producer.DisposeAsync();
    }
}
