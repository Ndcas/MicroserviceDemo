namespace OrderService.Application.Dtos;

public record OrderListResponseData(int Total, IReadOnlyList<OrderResponseData> Orders);
