using System.Net.Http.Json;
using Frontend.Constants;
using Frontend.Dtos;
using Frontend.Interfaces;

namespace Frontend.Api;

internal class OrderService : IOrderService
{
    private readonly HttpClient _client;

    public OrderService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(HttpClients.BusinessClientName);
    }

    public async Task<ApiResponse> CancelOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _client.PatchAsync(OrderServiceEndpoints.CancelOrder(id), null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse.Fail(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return ApiResponse.Success();
    }

    public async Task<ApiResponse> CompleteOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _client.PatchAsync(OrderServiceEndpoints.CompleteOrder(id), null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse.Fail(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return ApiResponse.Success();
    }

    public async Task<ApiResponse> ConfirmPaymentAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(OrderServiceEndpoints.ConfirmPayment(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse.Fail(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return ApiResponse.Success();
    }

    public async Task<ApiResponse<OrderData>> GetOrderDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(OrderServiceEndpoints.GetDetails(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<OrderData>.Fail(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return ApiResponse<OrderData>.Success(await response.Content.ReadFromJsonAsync<OrderData>(cancellationToken));
    }

    public async Task<ApiResponse<OrderListData>> GetOrdersAsync(int page = 1, int take = 10, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(OrderServiceEndpoints.GetAll(page, take), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<OrderListData>.Fail(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return ApiResponse<OrderListData>.Success(await response.Content.ReadFromJsonAsync<OrderListData>(cancellationToken));
    }

    public async Task<ApiResponse<PlaceOrderData>> PlaceOrderAsync(PlaceOrderRequest data, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(OrderServiceEndpoints.PlaceOrder, data, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<PlaceOrderData>.Fail(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return ApiResponse<PlaceOrderData>.Success(await response.Content.ReadFromJsonAsync<PlaceOrderData>(cancellationToken));
    }
}
