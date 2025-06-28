namespace TightlyCoupled.WebShop.Models
{
    /// <summary>
    /// Order entity for Entity Framework Core
    /// </summary>
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string ShippingOption { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Navigation properties
        public AppUser User { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Order Order { get; set; } = null!;
    }
}
