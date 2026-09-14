using System.Text.Json;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Constants;

namespace ProductService.Infrastructure.Broker;

public class OrderCreationConsumer : BackgroundService
{
    private readonly IPulsarClient _pulsarClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderCreationConsumer> _logger;

    public OrderCreationConsumer(
        IPulsarClient pulsarClient,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<OrderCreationConsumer> logger)
    {
        _pulsarClient = pulsarClient;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriptionName = _configuration[EnvironmentVariableKeys.BrokerSubscriptionName];
        var topic = _configuration[EnvironmentVariableKeys.OrderCreatedTopic];

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

                var productService = scope.ServiceProvider.GetRequiredService<IProductsService>();

                if (!message.Properties.TryGetValue(BrokerEventConfigurations.EventIdProperyKey, out var eventId))
                {
                    throw new InvalidDataException("Không tìm thấy Event Id");
                }

                if (!Guid.TryParse(eventId, out var guidEventId))
                {
                    throw new InvalidDataException("Không thể chuyển Event Id thành Guid");
                }

                var objMessage = JsonSerializer.Deserialize<OrderCreatedMessage>(message.Value());

                var response = await productService.PerformReservationAsync(guidEventId, objMessage, stoppingToken);

                if (!response.Ok)
                {
                    _logger.LogWarning(response.Error);
                }

                await consumer.Acknowledge(message, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                await consumer.RedeliverUnacknowledgedMessages(new[] { message.MessageId }, stoppingToken);
            }
        }
    }
}
