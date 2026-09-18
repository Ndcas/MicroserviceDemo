using OrderService.Application.Dtos;

namespace OrderService.Application.Interfaces;

public interface IOrderCancellationProducer : IAsyncDisposable
{
    Task SendAsync(Guid eventId, IReadOnlyList<ProductWithQuantityItem> message, CancellationToken cancellationToken = default);
}
