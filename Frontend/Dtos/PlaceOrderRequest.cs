namespace Frontend.Dtos;

public record PlaceOrderRequest(IReadOnlyList<ProductWithQuantityItem> Items);
