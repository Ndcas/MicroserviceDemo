using OrderService.Application.Dtos;

namespace OrderService.Application.Interfaces;

public interface IOrderCreationProducer : IAsyncDisposable
{
    Task SendAsync(Guid eventId, OrderCreatedMessage message, CancellationToken cancellationToken = default);
}
