using System.Net.Http.Json;
using Frontend.Constants;
using Frontend.Dtos;
using Frontend.Interfaces;

namespace Frontend.Api;

internal class ProductService : IProductService
{
    private readonly HttpClient _client;

    public ProductService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(HttpClients.BusinessClientName);
    }

    public async Task<ApiResponse<IReadOnlyList<AvailableProductItem>>> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(ProductServiceEndpoints.GetAvailableProducts, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<IReadOnlyList<AvailableProductItem>>.Fail(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return ApiResponse<IReadOnlyList<AvailableProductItem>>.Success(await response.Content.ReadFromJsonAsync<IReadOnlyList<AvailableProductItem>>(
            cancellationToken));
    }

    public async Task<ApiResponse<IReadOnlyList<CartProductItem>>> GetCartProductsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        var query = string.Join("&", productIds.Select(productId => $"ProductIds={productId}"));
        var response = await _client.GetAsync($"{ProductServiceEndpoints.GetCartProducts}?{query}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<IReadOnlyList<CartProductItem>>.Fail(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return ApiResponse<IReadOnlyList<CartProductItem>>.Success(await response.Content.ReadFromJsonAsync<IReadOnlyList<CartProductItem>>(
            cancellationToken));
    }

    public string GetProductImageUrl(string name)
    {
        return $"{_client.BaseAddress}{ProductServiceEndpoints.GetProductImage(name)}";
    }

    public string GetBrandImageUrl(string name)
    {
        return $"{_client.BaseAddress}{ProductServiceEndpoints.GetBrandImage(name)}";
    }
}
