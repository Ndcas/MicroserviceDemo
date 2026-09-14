using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.Dtos;

public record GetCartProductsRequest
{
    [Required(ErrorMessage = "Phải có mảng ID")]
    [MinLength(1, ErrorMessage = "Mảng phải có ít nhất 1 phần tử")]
    public List<int> Ids { get; init; }
}
