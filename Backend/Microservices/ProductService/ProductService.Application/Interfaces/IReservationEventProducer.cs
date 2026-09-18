using ProductService.Application.Dtos;

namespace ProductService.Application.Interfaces;

public interface IReservationEventProducer : IAsyncDisposable
{
    Task NotifyReservationSucceededAsync(
        Guid eventId,
        OrderIdMessage message,
        CancellationToken cancellationToken = default);

    Task NotifyReservationFailedAsync(
        Guid eventId,
        OrderIdMessage message,
        CancellationToken cancellationToken = default);
}
