using Application.UseCases.CalculateCartTotal;

namespace Application.Interfaces.UseCases;

public interface ICalculateCartTotalUseCase
{
    Task<decimal> CalculateTotalAsync(CalculateCartTotalInput input);
}
