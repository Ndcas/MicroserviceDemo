using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace log.Models;

[Table("log_levels")]
public partial class LogLevel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Level")]
    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
}
