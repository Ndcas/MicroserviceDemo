using System.Text.Json;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Application.Dtos;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Constants;

namespace OrderService.Infrastructure.Broker;

internal class FailedReservationEventConsumer : BackgroundService
{
    private readonly IPulsarClient _pulsarClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FailedReservationEventConsumer> _logger;

    public FailedReservationEventConsumer(
        IPulsarClient pulsarClient,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<FailedReservationEventConsumer> logger)
    {
        _pulsarClient = pulsarClient;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriptionName = _configuration[EnvironmentVariableKeys.BrokerSubscriptionName];
        var topic = _configuration[EnvironmentVariableKeys.ReservationFailedTopic];

        await using var consumer = _pulsarClient
            .NewConsumer(Schema.String)
            .SubscriptionName(subscriptionName)
            .Topic(topic)
            .Create();

        await foreach (var message in consumer.Messages(stoppingToken))
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var ordersService = scope.ServiceProvider.GetRequiredService<IOrdersService>();

                if (!message.Properties.TryGetValue(BrokerEventConfigurations.EventIdProperyKey, out var eventId))
                {
                    throw new InvalidDataException();
                }

                if (!Guid.TryParse(eventId, out var guidEventId))
                {
                    throw new InvalidDataException();
                }

                var objMessage = JsonSerializer.Deserialize<OrderIdMessage>(message.Value());

                var response = await ordersService.RemoveOrderAsync(guidEventId, objMessage, stoppingToken);

                if (!response.Ok)
                {
                    _logger.LogWarning(response.Error);
                }

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
