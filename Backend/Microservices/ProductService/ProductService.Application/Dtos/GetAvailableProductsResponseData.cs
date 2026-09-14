namespace ProductService.Application.Dtos;

public record GetAvailableProductsResponseData(IReadOnlyList<AvailableProductItem> Items);
