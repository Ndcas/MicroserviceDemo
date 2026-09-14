namespace ProductService.Application.Dtos;

public record GetCartProductsResponseData(IReadOnlyList<CartProductItem> Items);
