using OrderService.Application.Dtos;

namespace OrderService.Application.Interfaces;

public interface IOrdersService
{
    Task<ServiceResponse<PlaceOrderResponseData>> PlaceOrderAsync(
        int userId,
        IReadOnlyList<ProductWithQuantityItem> request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse> CancelOrderAsync(int userId, OrderIdMessage request, CancellationToken cancellationToken = default);

    Task<ServiceResponse> ConfirmReservationAsync(Guid eventId, OrderIdMessage request, CancellationToken cancellationToken = default);

    Task<ServiceResponse> ConfirmPaymentAsync(OrderIdMessage request, CancellationToken cancellationToken = default);

    Task<ServiceResponse> RemoveOrderAsync(Guid eventId, OrderIdMessage request, CancellationToken cancellationToken = default);

    Task<ServiceResponse> CompleteOrderASync(OrderIdMessage request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<OrderResponseData>> GetOrderDetailsAsync(int orderId, CancellationToken cancellationToken = default);

    Task<ServiceResponse<OrderResponseData>> GetOrderDetailsAsync(int userId, int orderId, CancellationToken cancellationToken = default);

    Task<ServiceResponse<IReadOnlyList<OrderResponseData>>> GetOrdersAsync(
        int page,
        int take,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<IReadOnlyList<OrderResponseData>>> GetOrdersAsync(
        int page,
        int take,
        int userId,
        CancellationToken cancellationToken = default);
}
