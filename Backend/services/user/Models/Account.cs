using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace user.Models;

[Table("account")]
[Index("RoleId", Name = "role_id")]
[Index("Username", Name = "username", IsUnique = true)]
public partial class Account
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("username")]
    public string Username { get; set; } = null!;

    [Column("password")]
    [StringLength(64)]
    public string Password { get; set; } = null!;

    [Column("is_active", TypeName = "enum('YES','NO')")]
    public string IsActive { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("Accounts")]
    public virtual Role Role { get; set; } = null!;
}
