namespace ProductService.Domain.Entities;

public partial class ProductType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();
}
