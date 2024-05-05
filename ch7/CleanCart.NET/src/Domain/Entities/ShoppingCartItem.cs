using System;

namespace Domain.Entities
{
    public class ShoppingCartItem
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public decimal ProductPrice { get; }
        public int Quantity { get; set; }

        public ShoppingCartItem(Guid productId, string productName, decimal productPrice, int quantity)
        {
            ProductId = productId;
            ProductName = productName;
            ProductPrice = productPrice;
            Quantity = quantity;
        }
    }
}
