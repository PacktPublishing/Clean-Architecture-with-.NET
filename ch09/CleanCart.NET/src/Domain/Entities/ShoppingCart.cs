namespace Domain.Entities;

public class ShoppingCart(Guid userId)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; } = userId;
    public List<ShoppingCartItem> Items { get; } = new();

    public void AddItem(Product product, int quantity)
    {
        var existingItem = Items.SingleOrDefault(i => i.ProductId == product.Id);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(new ShoppingCartItem(product.Id, product.Name, product.Price, quantity));
        }
    }

    public void RemoveItem(Guid productId, int quantity)
    {
        var existingItem = Items.SingleOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity -= quantity;

            if (existingItem.Quantity <= 0)
            {
                Items.Remove(existingItem);
            }
        }
    }
}