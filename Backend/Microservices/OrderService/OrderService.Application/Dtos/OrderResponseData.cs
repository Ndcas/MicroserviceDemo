namespace OrderService.Application.Dtos;

public record OrderResponseData(
    int OrderId,
    int UserId,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<OrderDetailItem>? OrderDetails);