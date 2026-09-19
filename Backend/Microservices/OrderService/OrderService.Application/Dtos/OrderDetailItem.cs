namespace OrderService.Application.Dtos;

public record OrderDetailItem(int ProductId, decimal PriceAtBooking, int Quantity);