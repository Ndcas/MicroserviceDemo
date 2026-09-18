using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.Dtos;

public record OrderIdMessage()
{
    [Required]
    public int OrderId { get; init; }
}
