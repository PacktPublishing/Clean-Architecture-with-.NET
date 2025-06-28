using Microsoft.AspNetCore.Identity;

namespace TightlyCoupled.WebShop.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public string Address { get; set; } = string.Empty;
        
        // Navigation property for orders
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
