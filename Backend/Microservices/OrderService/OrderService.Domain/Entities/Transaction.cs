namespace OrderService.Domain.Entities;

public partial class Transaction
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string TransactionRef { get; set; } = null!;

    public string? Status { get; set; }

    public string? Payload { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
