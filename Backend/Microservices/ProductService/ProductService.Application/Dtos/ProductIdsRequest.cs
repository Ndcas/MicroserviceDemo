using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.Dtos;

public record ProductIdsRequest()
{
    [Required]
    [MinLength(1)]
    public IReadOnlyList<int> ProductIds { get; init; }
}
