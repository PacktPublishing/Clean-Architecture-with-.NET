namespace Domain.Entities;

public class ShoppingCartItem(Guid productId, string productName, decimal productPrice, int quantity)
{
    public Guid ProductId { get; } = productId;
    public string ProductName { get; } = productName;
    public decimal ProductPrice { get; } = productPrice;
    public int Quantity { get; set; } = quantity;
}