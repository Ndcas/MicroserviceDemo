using Frontend.Authentication;
using Frontend.Dtos;
using Frontend.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Frontend.Pages;

public partial class Orders
{
    private const int ItemsPerPage = 10;
    private const int VisiblePage = 7;

    private string _userRole = string.Empty;
    private IReadOnlyList<OrderData> _orders = new List<OrderData>();
    private int _page = 1;
    private int _maxPage = 1;
    private string? _errorMessage;
    private int? _processingOrderId;
    private bool _isLoading = true;

    [Inject]
    private IOrderService OrderService { get; set; } = default;

    [Inject]
    private AuthenticationContext AuthenticationContext { get; set; } = default;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationContext.GetAuthenticationStateAsync();

        _userRole = authState.GetRoleId();

        await LoadOrdersAsync();
    }

    public async Task LoadOrdersAsync()
    {
        _isLoading = true;

        _errorMessage = null;

        var response = await OrderService.GetOrdersAsync(_page, ItemsPerPage);

        if (response.Ok)
        {
            _orders = response.Data.Orders;

            var total = response.Data.Total;

            _maxPage = Math.Max(1, (int)Math.Ceiling(total * 1.0 / ItemsPerPage));

            _page = Math.Min(_maxPage, _page);
        }
        else
        {
            _errorMessage = response.Error;
        }

        _isLoading = false;
    }

    public async Task GoToPageAsync(int page)
    {
        if (_isLoading || page < 1 || page > _maxPage || page == _page)
        {
            return;
        }

        _page = page;

        await LoadOrdersAsync();
    }

    public async Task CancelOrderAsync(int orderId)
    {
        _processingOrderId = orderId;

        _errorMessage = null;

        var response = await OrderService.CancelOrderAsync(orderId);

        if (!response.Ok)
        {
            _errorMessage = response.Error;

            _processingOrderId = null;

            return;
        }

        await LoadOrdersAsync();

        _processingOrderId = null;
    }

    public async Task PayOrderAsync(int orderId)
    {
        _processingOrderId = orderId;

        _errorMessage = null;

        var response = await OrderService.ConfirmPaymentAsync(orderId);

        if (!response.Ok)
        {
            _errorMessage = response.Error;

            _processingOrderId = null;

            return;
        }

        await LoadOrdersAsync();

        _processingOrderId = null;
    }

    public async Task CompleteOrderAsync(int orderId)
    {
        _processingOrderId = orderId;

        _errorMessage = null;

        var response = await OrderService.CompleteOrderAsync(orderId);

        if (!response.Ok)
        {
            _errorMessage = response.Error;

            _processingOrderId = null;

            return;
        }

        await LoadOrdersAsync();

        _processingOrderId = null;
    }
}
