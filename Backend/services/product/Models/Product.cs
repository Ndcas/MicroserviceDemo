using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace product.Models;

[Table("products")]
[Index("BrandId", Name = "brand_id")]
[Index("ProductTypeId", Name = "product_type_id")]
public partial class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_type_id")]
    public int ProductTypeId { get; set; }

    [Column("brand_id")]
    public int BrandId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("image")]
    [StringLength(255)]
    public string? Image { get; set; }

    [Column("price")]
    [Precision(10, 2)]
    public decimal Price { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("BrandId")]
    [InverseProperty("Products")]
    public virtual Brand Brand { get; set; } = null!;

    [ForeignKey("ProductTypeId")]
    [InverseProperty("Products")]
    public virtual ProductType ProductType { get; set; } = null!;
}
