using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.Dtos;

public record PlaceOrderRequest
{
    [Required()]
    [MinLength(1)]
    public IReadOnlyList<ProductWithQuantityItem> Items { get; init; }
}
