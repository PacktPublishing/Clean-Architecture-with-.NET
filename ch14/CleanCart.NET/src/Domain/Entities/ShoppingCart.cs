using EntityAxis.Abstractions;

namespace Domain.Entities;

public class ShoppingCart(Guid userId) : IEntityId<Guid>
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; } = userId;

    // Preserve encapsulation via a read-only view
    public IReadOnlyCollection<ShoppingCartItem> Items => _items.AsReadOnly();

    // Keep the mutable list private
    private readonly List<ShoppingCartItem> _items = new();

    public void AddItem(Guid productId, string productName, decimal productPrice, int quantity)
    {
        var existingItem = _items.SingleOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _items.Add(new ShoppingCartItem(productId, productName, productPrice, quantity));
        }
    }

    public void RemoveItem(Guid productId, int quantity)
    {
        var existingItem = _items.SingleOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity -= quantity;

            if (existingItem.Quantity <= 0)
            {
                _items.Remove(existingItem);
            }
        }
    }
}