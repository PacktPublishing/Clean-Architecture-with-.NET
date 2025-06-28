namespace TightlyCoupled.WebShop.Models
{
    /// <summary>
    /// Cart item entity for storing items in the shopping cart before checkout
    /// </summary>
    public class CartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public AppUser User { get; set; } = null!;
    }
}
