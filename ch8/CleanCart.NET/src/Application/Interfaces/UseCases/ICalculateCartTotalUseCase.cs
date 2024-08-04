using Application.UseCases.CalculateCartTotal;
using System.Threading.Tasks;

namespace Application.Interfaces.UseCases
{
    public interface ICalculateCartTotalUseCase
    {
        Task<decimal> CalculateTotalAsync(CalculateCartTotalInput input);
    }
}
