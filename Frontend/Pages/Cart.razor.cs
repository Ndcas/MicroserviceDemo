using Frontend.Constants;
using Frontend.Dtos;
using Frontend.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Frontend.Pages;

public partial class Cart
{
    private static TimeSpan PollingPeriod = TimeSpan.FromSeconds(1);

    private const int PollingTime = 15;

    private IReadOnlyList<CartProductItem> _products = [];
    private Dictionary<int, int> _quantities = [];
    private string? _errorMessage;
    private bool _isLoading = true;
    private bool _isPlacingOrder;

    [Inject]
    private ICartService CartService { get; set; } = default;

    [Inject]
    private IProductService ProductService { get; set; } = default;

    [Inject]
    private IOrderService OrderService { get; set; } = default;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default;

    protected override async Task OnInitializedAsync()
    {
        _quantities = CartService.GetCart();

        if (_quantities.Count == 0)
        {
            _isLoading = false;

            return;
        }

        var response = await ProductService.GetCartProductsAsync(_quantities.Keys.ToList());

        if (!response.Ok)
        {
            _errorMessage = response.Error;

            _isLoading = false;
        }

        _products = response.Data ?? new List<CartProductItem>();

        var idSet = _products
            .Select(product => product.Id)
            .ToHashSet();

        _quantities = _quantities
            .Where(quantity => idSet.Contains(quantity.Key))
            .ToDictionary<int, int>();

        CartService.SetCart(_quantities);

        _isLoading = false;
    }

    public decimal CartTotal()
    {
        return _products.Sum(product => product.Price * _quantities.GetValueOrDefault(product.Id));
    }

    public void IncreaseQuantity(int productId)
    {
        CartService.AddToCart(productId, 1);

        RefreshCart();
    }

    public void DecreaseQuantity(int productId)
    {
        CartService.RemoveFromCart(productId, 1);

        RefreshCart();
    }

    public void RefreshCart()
    {
        _quantities = CartService.GetCart();

        _products = _products
            .Where(product => _quantities.ContainsKey(product.Id))
            .ToList();
    }

    public async Task PlaceOrderAsync()
    {
        if (_quantities.Count == 0)
        {
            return;
        }

        _isPlacingOrder = true;
        _errorMessage = null;

        var items = _quantities
            .Where(item => item.Value > 0)
            .Select(item => new ProductWithQuantityItem(item.Key, item.Value))
            .ToList();

        var response = await OrderService.PlaceOrderAsync(new PlaceOrderRequest(items));

        if (!response.Ok)
        {
            _errorMessage = response.Error;
            _isLoading = false;
        }

        CartService.ClearCart();

        var pollingMessage = await PollingForOrderProcessingAsync(response.Data.OrderId);

        switch (pollingMessage)
        {
            case Messages.Success:
                NavigationManager.NavigateTo(Routes.Orders);

                return;
            case Messages.Failed:
            case Messages.Timeout:
                _isPlacingOrder = false;
                _errorMessage = pollingMessage;

                break;
        }
    }

    public async Task<string> PollingForOrderProcessingAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var timer = new PeriodicTimer(PollingPeriod);

        var checkTimes = 0;

        while (checkTimes < PollingTime && await timer.WaitForNextTickAsync(cancellationToken))
        {
            checkTimes++;

            var order = await OrderService.GetOrderDetailsAsync(orderId);

            if (!order.Ok)
            {
                continue;
            }

            if (order.Data?.Status == OrderStatuses.Unpaid)
            {
                return Messages.Success;
            }

            if (order.Data?.Status != OrderStatuses.Processing)
            {
                return Messages.Failed;
            }
        }

        return Messages.Timeout;
    }
}
