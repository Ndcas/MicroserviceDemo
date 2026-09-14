using ProductService.Application.Dtos;

namespace ProductService.Application.Interfaces;

public interface IProductsService
{
    Task<ServiceResponse<GetAvailableProductsResponseData>> GetAvailableProductsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResponse<GetCartProductsResponseData>> GetCartProductsAsync(
        GetCartProductsRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse> PerformReservationAsync(
        Guid eventId, 
        OrderCreatedMessage message, 
        CancellationToken cancellationToken = default);

    Task<ServiceResponse> PerformStocksSubstractionAsync(
        Guid eventId,
        PaymentCompletedMessage message,
        CancellationToken cancellationToken = default);
}
