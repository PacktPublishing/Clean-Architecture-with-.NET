using EntityAxis.Abstractions;

namespace Infrastructure.Persistence.Entities;

public class User : IEntityId<Guid>
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<Order>? NavOrders { get; set; }
    public ShoppingCart? NavShoppingCart { get; set; }
}