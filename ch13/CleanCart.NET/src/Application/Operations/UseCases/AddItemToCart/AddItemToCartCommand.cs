using MediatR;

namespace Application.Operations.UseCases.AddItemToCart;

public class AddItemToCartCommand(Guid userId, Guid productId, int quantity) : IRequest
{
    public Guid UserId { get; } = userId;

    public Guid ProductId { get; } = productId;

    public int Quantity { get; } = quantity;
}