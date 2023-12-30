using System;

namespace Application.UseCases.RemoveItemFromCart
{
    public class RemoveItemFromCartInput
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; }

        public RemoveItemFromCartInput(Guid customerId, Guid productId, int quantity)
        {
            CustomerId = customerId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
