namespace OrderService.Domain.Entities;

public partial class InboxMessage
{
    public Guid EventId { get; set; }

    public DateTime ProcessedAt { get; set; }
}
