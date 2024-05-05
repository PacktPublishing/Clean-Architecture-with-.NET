using Application.UseCases.CalculateCartTotal;
using System.Threading.Tasks;

namespace Application.Interfaces.UseCases
{
    public interface ICalculateCartTotalUseCase
    {
        Task<decimal> CalculateTotal(CalculateCartTotalInput input);
    }
}
