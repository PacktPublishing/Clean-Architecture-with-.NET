using System;

namespace Infrastructure.Persistence.Entities;

public class ShoppingCartItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public ShoppingCart NavShoppingCart { get; set; }
    public Product NavProduct { get; set; }
}