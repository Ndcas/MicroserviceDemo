using log.Models;

namespace log.Database
{
    public interface ILogRepository
    {
        void Create(Log log);
    }

    public class LogRepository : ILogRepository
    {
        private AppDbContext _context;

        public LogRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Create(Log log)
        {
            _context.Add(log);
        }
    }
}
