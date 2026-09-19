namespace Frontend.Dtos;

public record AvailableProductItem(
    int Id,
    int ProductTypeId,
    int BrandId,
    string Name,
    string? Image,
    decimal Price,
    int Availables);
