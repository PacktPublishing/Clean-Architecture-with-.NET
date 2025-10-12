using System;

namespace Application.UseCases.AddItemToCart;

public class AddItemToCartInput(Guid userId, Guid productId, int quantity)
{
    public Guid UserId { get; } = userId;
    public Guid ProductId { get; } = productId;
    public int Quantity { get; } = quantity;
}
