namespace Frontend.Dtos;

public record CartProductItem(
    int Id,
    string Name,
    string? Image,
    decimal Price,
    int Availables);
