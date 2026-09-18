using LogService.Domain.Entities;
using LogService.Domain.Interfaces;

namespace LogService.Infrastructure.Database.Repositories;

internal class LogRepository : ILogRepository
{
    private readonly LogServiceContext _context;

    public LogRepository(LogServiceContext context)
    {
        _context = context;
    }

    public Task<int> CreateAsync(Log log, CancellationToken cancellationToken = default)
    {
        _context.Add(log);

        return _context.SaveChangesAsync(cancellationToken);
    }
}

