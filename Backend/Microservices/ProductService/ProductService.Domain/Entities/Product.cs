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

    public int Stocks { get; set; }

    public int Reserved { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual ProductType ProductType { get; set; } = null!;

    public bool IsReservable(int quantity)
    {
        return Stocks - Reserved - quantity >= 0;
    }

    public void Reserve(int quantity)
    {
        if (!IsReservable(quantity))
        {
            throw new InvalidOperationException();
        }

        Reserved += quantity;
    }

    public void SubstractPaidStocks(int quantity)
    {
        if (Stocks - quantity < 0 || Reserved - quantity < 0)
        {
            throw new InvalidOperationException();
        }

        Stocks -= quantity;
        Reserved -= quantity;
    }

    public void Unreserve(int quantity)
    {
        Reserved -= quantity;
    }
}
