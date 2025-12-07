namespace Infrastructure.Persistence.Entities;

public class OrderItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public Product? NavProduct { get; set; }
    public Order? NavOrder { get; set; }
}