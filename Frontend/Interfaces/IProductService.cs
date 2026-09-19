using Frontend.Dtos;

namespace Frontend.Interfaces;

internal interface IProductService
{
    Task<ApiResponse<IReadOnlyList<AvailableProductItem>>> GetAvailableProductsAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<CartProductItem>>> GetCartProductsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default);

    string GetProductImageUrl(string name);

    string GetBrandImageUrl(string name);
}
