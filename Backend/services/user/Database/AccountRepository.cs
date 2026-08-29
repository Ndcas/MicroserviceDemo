using Microsoft.EntityFrameworkCore;
using user.Models;
using user.Utils;

namespace user.Database
{
    public interface IAccountRepository
    {
        Task<Account?> GetAccountByUsername(string username, LockMode lockMode = LockMode.None);
    }

    public class AccountRepository : IAccountRepository
    {
        private AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetAccountByUsername(string username, LockMode lockMode = LockMode.None)
        {
            return lockMode switch
            {
                LockMode.PessimisticRead => await _context.Accounts
                    .FromSqlInterpolated($"SELECT * FROM account WHERE username {username} FOR SHARE")
                    .FirstOrDefaultAsync(),
                LockMode.PessimisticWrite => await _context.Accounts
                    .FromSqlInterpolated($"SELECT * FROM account WHERE username = {username} FOR UPDATE")
                    .FirstOrDefaultAsync(),
                _ => await _context.Accounts.FirstOrDefaultAsync(a => a.Username == username),
            };
        }
    }
}
