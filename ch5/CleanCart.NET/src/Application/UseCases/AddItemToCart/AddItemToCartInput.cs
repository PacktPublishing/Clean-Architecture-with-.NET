using System;

namespace Application.UseCases.AddItemToCart
{
    public class AddItemToCartInput
    {
        public Guid CustomerId { get; }
        public Guid ProductId { get; }
        public int Quantity { get; }

        public AddItemToCartInput(Guid customerId, Guid productId, int quantity)
        {
            CustomerId = customerId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
