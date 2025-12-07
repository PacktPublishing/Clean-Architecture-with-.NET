using MediatR;

namespace Application.Operations.UseCases.CalculateCartTotal;

public class CalculateCartTotalQuery(Guid userId) : IRequest<decimal>
{
    public Guid UserId { get; } = userId;
}