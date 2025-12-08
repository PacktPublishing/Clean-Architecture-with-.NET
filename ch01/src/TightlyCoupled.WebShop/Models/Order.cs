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
        public string CourierService { get; set; } = string.Empty;
        public bool IsVipOrder { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Navigation properties
        public AppUser User { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
