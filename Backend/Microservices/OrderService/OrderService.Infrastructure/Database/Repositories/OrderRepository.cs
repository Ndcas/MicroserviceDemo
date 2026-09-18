using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Infrastructure.Database.Repositories;

internal class OrderRepository : IOrderRepository
{
    private readonly OrderServiceContext _context;

    public OrderRepository(OrderServiceContext context)
    {
        _context = context;
    }

    public void AddOrder(Order order)
    {
        _context.Add(order);
    }

    public async Task<Order?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(order => order.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FromSqlRaw($"SELECT * FROM orders WHERE id = {id} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderWithDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(order => order.Id == id)
            .Include(order => order.OrderDetails)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderWithDetailsByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FromSqlRaw($"SELECT * FROM orders WHERE id = {id} FOR UPDATE")
            .Include(order => order.OrderDetails)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task RemoveOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            await PerformOrderRemoval(order, cancellationToken);

            DetachOrder(order);

            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await PerformOrderRemoval(order, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            DetachOrder(order);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task<List<Order>> GetOrderListAsync(int page, int take, CancellationToken cancellationToken = default)
    {
        var offset = (page - 1) * take;

        return await _context.Orders
            .Skip(offset)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrderListAsync(
        int userId,
        int page,
        int take,
        CancellationToken cancellationToken = default)
    {
        var offset = (page - 1) * take;

        return await _context.Orders
            .Where(order => order.UserId == userId)
            .Skip(offset)
            .Take(take)
            .ToListAsync();
    }

    private async Task PerformOrderRemoval(Order order, CancellationToken cancellationToken = default)
    {
        await _context.OrderDetails
            .Where(detail => detail.OrderId == order.Id)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Orders
            .Where(o => o.Id == order.Id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private void DetachOrder(Order order)
    {
        var entry = _context.Entry(order);

        foreach (var detail in order.OrderDetails)
        {
            _context.Entry(detail).State = EntityState.Detached;
        }

        entry.State = EntityState.Detached;
    }
}
