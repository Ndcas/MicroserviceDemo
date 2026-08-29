using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace product.Models;

[Table("product_types")]
public partial class ProductType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("ProductType")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [ForeignKey("ProductTypeId")]
    [InverseProperty("ProductTypes")]
    public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();
}
