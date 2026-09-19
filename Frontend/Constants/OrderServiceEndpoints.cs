namespace Frontend.Constants;

internal static class OrderServiceEndpoints
{
    public const string PlaceOrder = "Order";

    public static string CancelOrder(int id)
    {
        return $"Order/Cancel/{id}";
    }

    public static string CompleteOrder(int id)
    {
        return $"Order/Complete/{id}";
    }

    public static string GetDetails(int id)
    {
        return $"Order/{id}";
    }

    public static string GetAll(int page = 1, int take = 10)
    {
        return $"Order?page={page}&take={take}";
    }

    public static string ConfirmPayment(int id)
    {
        return $"Order/Pay/{id}";
    }
}
