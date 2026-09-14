using System.Text;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Infrastructure.Database.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductServiceContext _context;

    public ProductRepository(ProductServiceContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(product => product.Stocks > product.Reserved)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetProductsByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(product => ids.Contains(product.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetProductsByIdsForUpdateAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
    {
        if (ids is null || ids.Count == 0)
        {
            return new List<Product>();
        }

        var processedIds = ids
            .Distinct()
            .OrderBy(id => id);

        var inClause = $"IN ({string.Join(',', processedIds)})";

        return await _context.Products
            .FromSqlRaw($"SELECT * FROM products WHERE id {inClause} FOR UPDATE")
            .ToListAsync(cancellationToken);
    }
}
