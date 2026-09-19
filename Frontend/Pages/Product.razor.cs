using Frontend.Dtos;
using Frontend.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Frontend.Pages;

public partial class Product
{
    private IReadOnlyList<AvailableProductItem> _products = new List<AvailableProductItem>();
    private string? _errorMessage;
    private bool _isLoading = true;

    [Inject]
    private IProductService ProductService { get; set; } = default;

    [Inject]
    private ICartService CartService { get; set; } = default;

    protected override async Task OnInitializedAsync()
    {
        var response = await ProductService.GetAvailableProductsAsync();

        if (response.Ok)
        {
            _products = response.Data ?? new List<AvailableProductItem>();
        }
        else
        {
            _errorMessage = response.Error ?? string.Empty;
        }

        _isLoading = false;
    }

    public void AddToCart(int productId)
    {
        CartService.AddToCart(productId, 1);
    }
}
