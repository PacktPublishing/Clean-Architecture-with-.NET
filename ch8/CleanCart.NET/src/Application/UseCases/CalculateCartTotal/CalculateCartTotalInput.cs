using System;

namespace Application.UseCases.CalculateCartTotal
{
    public class CalculateCartTotalInput
    {
        public Guid UserId { get; }

        public CalculateCartTotalInput(Guid userId)
        {
            UserId = userId;
        }
    }
}
