namespace ProductService.Domain.Entities;

public partial class Product
{
    public int Id { get; set; }

    public int ProductTypeId { get; set; }

    public int BrandId { get; set; }

    public string Name { get; set; } = null!;

    public string? Image { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual ProductType ProductType { get; set; } = null!;
}
