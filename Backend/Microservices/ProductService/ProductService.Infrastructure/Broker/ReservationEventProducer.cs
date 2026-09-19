using System.Text.Json;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Configuration;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Constants;

namespace ProductService.Infrastructure.Broker;

internal class ReservationEventProducer : IReservationEventProducer
{
    private readonly IProducer<string> _succeededProducer;
    private readonly IProducer<string> _failedProducer;

    public ReservationEventProducer(IPulsarClient pulsarClient, IConfiguration configuration)
    {
        var reservationCompletedTopic = configuration[EnvironmentVariableKeys.ReservationCompletedTopic];
        var reservationFailedTopic = configuration[EnvironmentVariableKeys.ReservationFailedTopic];

        _succeededProducer = pulsarClient.NewProducer(Schema.String).Topic(reservationCompletedTopic).Create();
        _failedProducer = pulsarClient.NewProducer(Schema.String).Topic(reservationFailedTopic).Create();
    }

    public async Task NotifyReservationFailedAsync(
        Guid eventId,
        OrderIdMessage message,
        CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonSerializer.Serialize(message);

        await _failedProducer
            .NewMessage()
            .Property(BrokerEventConfigurations.EventIdProperyKey, eventId.ToString())
            .Send(jsonContent, cancellationToken);
    }

    public async Task NotifyReservationSucceededAsync(
        Guid eventId,
        OrderIdMessage message,
        CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonSerializer.Serialize(message);

        await _succeededProducer
            .NewMessage()
            .Property(BrokerEventConfigurations.EventIdProperyKey, eventId.ToString())
            .Send(jsonContent, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _succeededProducer.DisposeAsync();
        }
        finally
        {
            await _failedProducer.DisposeAsync();
        }
    }
}
