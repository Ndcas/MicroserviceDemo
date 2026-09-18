using ProductService.Application.Dtos;

namespace ProductService.Application.Interfaces;

public interface IProductsService
{
    Task<ServiceResponse<IReadOnlyList<AvailableProductItem>>> GetAvailableProductsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResponse<IReadOnlyList<CartProductItem>>> GetCartProductsAsync(
        ProductIdsRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse> PerformReservationAsync(
        Guid eventId, 
        OrderCreatedMessage message, 
        CancellationToken cancellationToken = default);

    Task<ServiceResponse> PerformStocksSubstractionAsync(
        Guid eventId,
        IReadOnlyList<ProductWithQuantityItem> message,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse> PerformStocksUnreservationAsync(
        Guid eventId,
        IReadOnlyList<ProductWithQuantityItem> message,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<IReadOnlyList<ProductWithPriceItem>>> GetProductPriceAsync(
        IReadOnlyList<int> ids,
        CancellationToken cancellationToken = default);
}
