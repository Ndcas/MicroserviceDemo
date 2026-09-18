using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Database.Repositories;

internal class AccountRepository : IAccountRepository
{
    private readonly UserServiceContext _context;

    public AccountRepository(UserServiceContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Username == username, cancellationToken);
    }
}
