namespace TightlyCoupled.WebShop.ViewModels;

public class CartItemViewModel
{
    public string ItemName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public decimal Subtotal => Price * Quantity;
}
