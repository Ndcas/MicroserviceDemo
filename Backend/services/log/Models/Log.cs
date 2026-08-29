using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace log.Models;

[Table("logs")]
[Index("LevelId", Name = "level_id")]
public partial class Log
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("level_id")]
    public int LevelId { get; set; }

    [Column("source")]
    [StringLength(255)]
    public string Source { get; set; } = null!;

    [Column("correlation_id")]
    [StringLength(36)]
    public string CorrelationId { get; set; } = null!;

    [Column("ip")]
    [StringLength(15)]
    public string Ip { get; set; } = null!;

    [Column("time", TypeName = "timestamp")]
    public DateTime Time { get; set; }

    [Column("content", TypeName = "text")]
    public string Content { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("LevelId")]
    [InverseProperty("Logs")]
    public virtual LogLevel Level { get; set; } = null!;
}
