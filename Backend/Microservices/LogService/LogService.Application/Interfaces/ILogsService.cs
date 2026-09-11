using LogService.Application.Dtos;

namespace LogService.Application.Interfaces;

public interface ILogsService
{
    Task WriteLogAsync(LogMessage log, CancellationToken cancellationToken = default);
}

