using System;
using MediatR;

namespace Application.Operations.UseCases.CalculateCartTotal
{
    public class CalculateCartTotalQuery : IRequest<decimal>
    {
        public CalculateCartTotalQuery(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}
