using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}
