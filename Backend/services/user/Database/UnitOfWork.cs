using Microsoft.EntityFrameworkCore.Storage;

namespace user.Database
{
    public interface IUnitOfWork : IDisposable
    {
        Task BeginTransaction();

        Task RollbackTransaction();

        Task CommitTransaction();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransaction()
        {
            if (_transaction != null)
            {
                return;
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task RollbackTransaction()
        {
            if (_transaction == null)
            {
                return;
            }

            await _transaction.RollbackAsync();

            _transaction = null;
        }

        public async Task CommitTransaction()
        {
            try
            {
                await _context.SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransaction();

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
}
