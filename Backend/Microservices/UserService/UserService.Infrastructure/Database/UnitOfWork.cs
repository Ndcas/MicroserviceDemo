using Microsoft.EntityFrameworkCore.Storage;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Database;

public class UnitOfWork : IUnitOfWork
{
    private readonly UserServiceContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(UserServiceContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            return;
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);

        _transaction = null;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);

            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();

                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
    }
}
