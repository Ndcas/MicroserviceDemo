using ProductService.Domain.Entities;

namespace ProductService.Domain.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAvailableProductsAsync(CancellationToken cancellationToken = default);

    Task<List<Product>> GetProductsByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);

    Task<List<Product>> GetProductsByIdsForUpdateAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);
}
