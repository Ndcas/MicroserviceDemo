namespace LogService.Domain.Entities
{
    public partial class Log
    {
        public int Id { get; set; }

        public int LevelId { get; set; }

        public string Source { get; set; } = null!;

        public string CorrelationId { get; set; } = null!;

        public string Ip { get; set; } = null!;

        public DateTime Time { get; set; }

        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public virtual LogLevel Level { get; set; } = null!;
    }
}
