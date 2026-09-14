using ProductService.Application.Dtos;

namespace ProductService.Application.Interfaces;

public interface IReservationEventProducer
{
    Task NotifyReservationSucceededAsync(ReservationEventMessage message, CancellationToken cancellationToken = default);

    Task NotifyReservationFailedAsync(ReservationEventMessage message, CancellationToken cancellationToken = default);
}
