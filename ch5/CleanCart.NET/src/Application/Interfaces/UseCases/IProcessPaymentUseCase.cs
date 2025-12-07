using Application.UseCases.ProcessPayment;
using Domain.Entities;

namespace Application.Interfaces.UseCases;

public interface IProcessPaymentUseCase
{
    Task<Order> ProcessPaymentAsync(ProcessPaymentInput input);
}
