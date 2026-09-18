using System.Text.Json;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using LogService.Application.Dtos;
using LogService.Application.Interfaces;
using LogService.Infrastructure.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogService.Infrastructure.Broker;

internal class LogConsumer : BackgroundService
{
    private readonly IPulsarClient _client;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LogConsumer> _logger;

    public LogConsumer(
        IPulsarClient client,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<LogConsumer> logger)
    {
        _client = client;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriptionName = _configuration[EnvironmentVariableKeys.BrokerSubscriptionName];
        var topic = _configuration[EnvironmentVariableKeys.BrokerTopic];

        await using var consumer = _client
            .NewConsumer(Schema.String)
            .SubscriptionName(subscriptionName)
            .Topic(topic)
            .Create();

        await foreach (var message in consumer.Messages(stoppingToken))
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var logService = scope.ServiceProvider.GetRequiredService<ILogsService>();

                var logMessage = JsonSerializer.Deserialize<LogMessage>(message.Value());

                await logService.WriteLogAsync(logMessage, stoppingToken);

                await consumer.Acknowledge(message, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                await consumer.RedeliverUnacknowledgedMessages(new[] { message.MessageId }, stoppingToken);
            }
        }
    }
}

