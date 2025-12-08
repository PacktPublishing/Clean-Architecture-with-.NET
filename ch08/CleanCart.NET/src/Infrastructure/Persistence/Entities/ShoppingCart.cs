namespace Infrastructure.Persistence.Entities;

public class ShoppingCart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<ShoppingCartItem> Items { get; set; } = new();
    public User? NavUser { get; set; }
}