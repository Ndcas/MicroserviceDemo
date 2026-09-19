using Frontend.Constants;
using Frontend.Interfaces;
using Microsoft.JSInterop;

namespace Frontend.Services;

internal class CartService : ICartService
{
    private readonly ILocalStorageService _localStorageService;

    public CartService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public void AddToCart(int productId, int quantity)
    {
        var cart = GetCart();

        if (!cart.TryAdd(productId, quantity))
        {
            cart[productId] += quantity;
        }

        _localStorageService.SetItem(LocalStorageKeys.Cart, cart);
    }

    public Dictionary<int, int> GetCart()
    {
        var cart = _localStorageService.GetItem<Dictionary<int, int>>(LocalStorageKeys.Cart);

        if (cart is null)
        {
            cart = new Dictionary<int, int>();

            _localStorageService.SetItem(LocalStorageKeys.Cart, cart);
        }

        return cart;
    }

    public void RemoveFromCart(int productId, int? quantity = null)
    {
        var cart = GetCart();

        if (!cart.ContainsKey(productId))
        {
            return;
        }

        cart[productId] = Math.Max(cart[productId] - quantity ?? cart[productId], 0);

        if (cart[productId] == 0)
        {
            cart.Remove(productId);
        }

        _localStorageService.SetItem(LocalStorageKeys.Cart, cart);
    }

    public void SetCart(IReadOnlyDictionary<int, int> cart)
    {
        _localStorageService.SetItem(LocalStorageKeys.Cart, cart);
    }

    public void ClearCart()
    {
        _localStorageService.RemoveItem(LocalStorageKeys.Cart);
    }
}
