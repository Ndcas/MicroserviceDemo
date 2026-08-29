using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace order.Models;

[Table("transactions")]
[Index("OrderId", Name = "order_id")]
public partial class Transaction
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("transaction_ref")]
    [StringLength(255)]
    public string TransactionRef { get; set; } = null!;

    [Column("status", TypeName = "enum('SUCCESS','FAIL')")]
    public string? Status { get; set; }

    [Column("payload", TypeName = "text")]
    public string? Payload { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("Transactions")]
    public virtual Order Order { get; set; } = null!;
}
