namespace OrderService.Application.Dtos;

public record OrderCreatedMessage(int OrderId, IReadOnlyList<ProductWithQuantityItem> Items);
