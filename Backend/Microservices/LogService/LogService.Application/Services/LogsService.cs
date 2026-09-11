using LogService.Application.Constants;
using LogService.Application.Dtos;
using LogService.Application.Interfaces;
using LogService.Domain.Entities;
using LogService.Domain.Interfaces;

namespace LogService.Application.Services;

public class LogsService : ILogsService
{
    private readonly ILogRepository _logRepository;

    public LogsService(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task WriteLogAsync(LogMessage logMessage, CancellationToken cancellationToken = default)
    {
        int levelId = (int)LogLevelEnum.Information;

        switch (logMessage.Level)
        {
            case nameof(LogLevelEnum.Warning):
                levelId = (int)LogLevelEnum.Warning;

                break;
            case nameof(LogLevelEnum.Error):
                levelId = (int)LogLevelEnum.Error;

                break;
            default:
                break;
        }

        Log log = new Log
        {
            LevelId = levelId,
            Source = logMessage.Source,
            CorrelationId = logMessage.CorrelationId,
            Ip = logMessage.Ip,
            Time = DateTime.Parse(logMessage.Time),
            Content = logMessage.Content
        };

        await _logRepository.CreateAsync(log, cancellationToken);
    }
}

