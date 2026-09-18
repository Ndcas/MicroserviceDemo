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

internal class PaymentCompletionConsumer : BackgroundService
{
    private readonly IPulsarClient _pulsarClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentCompletionConsumer> _logger;

    public PaymentCompletionConsumer(
        IPulsarClient pulsarClient,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<PaymentCompletionConsumer> logger)
    {
        _pulsarClient = pulsarClient;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriptionName = _configuration[EnvironmentVariableKeys.BrokerSubscriptionName];
        var topic = _configuration[EnvironmentVariableKeys.PaymentCompletedTopic];

        await using var consumer = _pulsarClient
            .NewConsumer(Schema.String)
            .SubscriptionName(subscriptionName)
            .Topic(topic)
            .Create();

        await foreach (var message in consumer.Messages(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var productService = scope.ServiceProvider.GetRequiredService<IProductsService>();

                if (!message.Properties.TryGetValue(BrokerEventConfigurations.EventIdProperyKey, out var eventId))
                {
                    throw new InvalidDataException(nameof(eventId));
                }

                if (!Guid.TryParse(eventId, out var guidEventId))
                {
                    throw new InvalidDataException(nameof(guidEventId));
                }

                var objMessage = JsonSerializer.Deserialize<IReadOnlyList<ProductWithQuantityItem>>(message.Value());

                var response = await productService.PerformStocksSubstractionAsync(guidEventId, objMessage, stoppingToken);

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
