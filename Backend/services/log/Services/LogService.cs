using log.Database;
using log.Models;

namespace log.Services
{
    public interface ILogService
    {
        Task WriteLog(string level, string source, string correlationId, string ip, string time, string content);
    }

    public class LogService : ILogService
    {
        private ILogRepository _logRepository;
        private IUnitOfWork _unitOfWork;
        private ILogger<LogService> _logger;

        public LogService(ILogRepository logRepository, IUnitOfWork unitOfWork, ILogger<LogService> logger)
        {
            _logRepository = logRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task WriteLog(string level, string source, string correlationId, string ip, string time, string content)
        {
            string message = $"{time}\t{ip}\t{correlationId}\t{source}\t{content}";
            int levelId = 1;

            switch (level)
            {
                case "Warning":
                    _logger.LogWarning(message);
                    levelId = 2;
                    break;
                case "Error":
                    _logger.LogError(message);
                    levelId = 3;
                    break;
                default:
                    _logger.LogInformation(message);
                    break;
            }

            Log log = new Log
            {
                LevelId = levelId,
                Source = source,
                CorrelationId = correlationId,
                Ip = ip,
                Time = DateTime.Parse(time),
                Content = content
            };

            _logRepository.Create(log);

            await _unitOfWork.CommitTransaction();
        }
    }
}
