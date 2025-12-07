using MediatR;

namespace Application.Operations.UseCases.RemoveItemFromCart;

public class RemoveItemFromCartCommand(Guid userId, Guid productId, int quantity) : IRequest
{
    public Guid UserId { get; set; } = userId;

    public Guid ProductId { get; set; } = productId;

    public int Quantity { get; } = quantity;
}