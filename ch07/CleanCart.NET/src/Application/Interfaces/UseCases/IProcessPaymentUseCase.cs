using Application.UseCases.ProcessPayment;

namespace Application.Interfaces.UseCases;

public interface IProcessPaymentUseCase
{
    Task ProcessPaymentAsync(ProcessPaymentInput input);
}