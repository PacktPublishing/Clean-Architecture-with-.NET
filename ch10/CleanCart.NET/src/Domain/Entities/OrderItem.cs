using System;

namespace Domain.Entities
{
    public class OrderItem
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        public decimal ProductPrice { get; }
        public int Quantity { get; }

        public OrderItem(Guid productId, string productName, decimal productPrice, int quantity)
        {
            ProductId = productId;
            ProductName = productName;
            ProductPrice = productPrice;
            Quantity = quantity;
        }
    }
}
