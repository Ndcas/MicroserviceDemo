using OrderService.Domain.Entities;

namespace OrderService.Domain.Interfaces;

public interface IOrderRepository
{
    void AddOrder(Order order);

    Task<Order?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Order?> GetOrderByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);

    Task<Order?> GetOrderWithDetailsByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Order?> GetOrderWithDetailsByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);

    Task RemoveOrderAsync(Order order, CancellationToken cancellationToken = default);

    Task<List<Order>> GetOrderListAsync(int page, int take, CancellationToken cancellationToken = default);

    Task<List<Order>> GetOrderListAsync(
        int userId,
        int page,
        int take,
        CancellationToken cancellationToken = default);
}
