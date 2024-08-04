using System;

namespace Application.UseCases.RemoveItemFromCart
{
    public class RemoveItemFromCartInput
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; }

        public RemoveItemFromCartInput(Guid userId, Guid productId, int quantity)
        {
            UserId = userId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
