using System.Text.Json;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Configuration;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Constants;

namespace UserService.Infrastructure.Broker;

public class LogProducer : ILogProducer
{
    private IProducer<string> _producer;

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

    public ValueTask DisposeAsync()
    {
        return _producer.DisposeAsync();
    }
}
