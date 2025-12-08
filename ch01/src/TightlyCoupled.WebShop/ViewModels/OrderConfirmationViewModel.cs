namespace TightlyCoupled.WebShop.ViewModels;

public class OrderConfirmationViewModel
{
    public int OrderId { get; set; }
    public string CustomerEmail { get; set; } = "";
    public string CustomerAddress { get; set; } = "";
    public string ShippingOption { get; set; } = "";
    public string Courier { get; set; } = "";
    public DateTime EstimatedDeliveryDate { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalPrice { get; set; }

    public List<CartItemViewModel> Items { get; set; } = new();
}
