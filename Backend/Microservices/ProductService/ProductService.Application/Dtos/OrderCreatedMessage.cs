namespace ProductService.Application.Dtos;

public record OrderCreatedMessage(int OrderId, IReadOnlyList<ProductWithQuantityItem> Items);
