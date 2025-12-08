namespace Application.UseCases.RemoveItemFromCart;

public class RemoveItemFromCartInput(Guid userId, Guid productId, int quantity)
{
    public Guid UserId { get; set; } = userId;
    public Guid ProductId { get; set; } = productId;
    public int Quantity { get; } = quantity;
}
