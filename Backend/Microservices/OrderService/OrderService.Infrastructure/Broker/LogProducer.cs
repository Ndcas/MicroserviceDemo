using System.Text.Json;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Configuration;
using OrderService.Application.Dtos;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Constants;

namespace OrderService.Infrastructure.Broker;

internal class LogProducer : ILogProducer
{
    private readonly IProducer<string> _producer;

    public LogProducer(IPulsarClient client, IConfiguration configuration)
    {
        var logTopic = configuration[EnvironmentVariableKeys.BrokerLogSentTopic];

        _producer = client.NewProducer(Schema.String).Topic(logTopic).Create();
    }

    public async Task SendAsync(LogMessage logMessage, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonSerializer.Serialize(logMessage);

        await _producer.NewMessage().Send(jsonContent, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _producer.DisposeAsync();
    }
}
