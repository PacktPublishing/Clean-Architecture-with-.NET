using Application.UseCases.ProcessPayment;
using System.Threading.Tasks;

namespace Application.Interfaces.UseCases
{
    public interface IProcessPaymentUseCase
    {
        Task ProcessPaymentAsync(ProcessPaymentInput input);
    }
}