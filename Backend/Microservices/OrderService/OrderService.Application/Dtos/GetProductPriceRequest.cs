using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.Dtos;

public record GetProductPriceRequest()
{
    [Required]
    [MinLength(1)]
    public IReadOnlyList<int> ProductIds { get; init; }
}
