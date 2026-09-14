using System.Text.Json;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Configuration;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Constants;

namespace ProductService.Infrastructure.Broker;

public class ReservationEventProducer : IReservationEventProducer
{
    private IProducer<string> _succeededProducer;
    private IProducer<string> _failedProducer;

    public ReservationEventProducer(IPulsarClient pulsarClient, IConfiguration configuration)
    {
        var reservationCompletedTopic = configuration[EnvironmentVariableKeys.ReservationCompletedTopic];
        var reservationFailedTopic = configuration[EnvironmentVariableKeys.ReservationFailedTopic];

        _succeededProducer = pulsarClient.NewProducer(Schema.String).Topic(reservationCompletedTopic).Create();
        _failedProducer = pulsarClient.NewProducer(Schema.String).Topic(reservationFailedTopic).Create();
    }

    public async Task NotifyReservationFailedAsync(ReservationEventMessage message, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonSerializer.Serialize(message);

        await _succeededProducer.NewMessage().Send(jsonContent, cancellationToken);
    }

    public async Task NotifyReservationSucceededAsync(ReservationEventMessage message, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonSerializer.Serialize(message);

        await _failedProducer.NewMessage().Send(jsonContent, cancellationToken);
    }
}
