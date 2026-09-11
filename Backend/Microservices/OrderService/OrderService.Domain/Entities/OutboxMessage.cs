namespace OrderService.Domain.Entities;

public partial class OutboxMessage
{
    public int Id { get; set; }

    public Guid EventId { get; set; }

    public string Topic { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }
}
