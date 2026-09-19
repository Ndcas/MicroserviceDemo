namespace Frontend.Constants;

internal static class ProductServiceEndpoints
{
    public const string GetAvailableProducts = "Product/Available";
    public const string GetCartProducts = "Product/Cart";

    public static string GetProductImage(string name)
    {
        return $"Product/Image/Product/{name}";
    }

    public static string GetBrandImage(string name)
    {
        return $"Product/Image/Brand/{name}";
    }
}
