namespace Frontend.Interfaces;

public interface ICartService
{
    void AddToCart(int productId, int quantity);

    void RemoveFromCart(int productId, int? quantity = null);

    Dictionary<int, int> GetCart();

    void SetCart(IReadOnlyDictionary<int, int> cart);

    void ClearCart();
}
