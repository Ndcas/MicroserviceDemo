namespace LogService.Domain.Entities
{
    public partial class LogLevel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
    }
}
