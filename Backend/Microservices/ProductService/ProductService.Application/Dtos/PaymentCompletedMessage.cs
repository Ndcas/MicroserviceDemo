namespace ProductService.Application.Dtos;

public record PaymentCompletedMessage(IReadOnlyList<ProductWithQuantityItem> Items);
