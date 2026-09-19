using Frontend.Dtos;

namespace Frontend.Interfaces;

internal interface IOrderService
{
    Task<ApiResponse<PlaceOrderData>> PlaceOrderAsync(PlaceOrderRequest data, CancellationToken cancellationToken = default);

    Task<ApiResponse> CancelOrderAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResponse> ConfirmPaymentAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResponse> CompleteOrderAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResponse<OrderListData>> GetOrdersAsync(int page, int take, CancellationToken cancellationToken = default);

    Task<ApiResponse<OrderData>> GetOrderDetailsAsync(int id, CancellationToken cancellationToken = default);
}
