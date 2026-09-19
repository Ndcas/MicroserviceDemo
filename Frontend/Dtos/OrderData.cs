namespace Frontend.Dtos;

public record OrderData(
    int OrderId,
    int UserId,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<OrderDetailItem>? OrderDetails);