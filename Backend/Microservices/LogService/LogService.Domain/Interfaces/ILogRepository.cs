using LogService.Domain.Entities;

namespace LogService.Domain.Interfaces;

public interface ILogRepository
{
    Task<int> CreateAsync(Log log, CancellationToken cancellationToken = default);
}

