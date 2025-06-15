using EntityAxis.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities
{
    public class ShoppingCart : IEntityId<Guid>
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public List<ShoppingCartItem> Items { get; }

        public ShoppingCart(Guid userId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Items = new List<ShoppingCartItem>();
        }

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
}
