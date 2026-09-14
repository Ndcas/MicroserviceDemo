namespace ProductService.Application.Dtos;

public record CartProductItem(
    int Id,
    string Name,
    string? Image,
    decimal Price,
    int Availables);
