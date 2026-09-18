using OrderService.Domain.Constants;

namespace OrderService.Domain.Entities;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public void Cancel()
    {
        Status = OrderStatuses.Canceled;
    }

    public void WaitForPayment()
    {
        Status = OrderStatuses.Unpaid;
    }

    public void ConfirmPayment()
    {
        Status = OrderStatuses.Confirmed;
    }

    public void Finish()
    {
        Status = OrderStatuses.Finished;
    }

    public void AddDetails(OrderDetail details)
    {
        OrderDetails.Add(details);
    }

    public void AddDetails(IEnumerable<OrderDetail> details)
    {
        foreach (var item in details)
        {
            AddDetails(item);
        }
    }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);
    }
}
