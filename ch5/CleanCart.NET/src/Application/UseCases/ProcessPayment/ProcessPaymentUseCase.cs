using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.UseCases;
using Application.UseCases.CalculateCartTotal;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Threading.Tasks;

namespace Application.UseCases.ProcessPayment
{
    public class ProcessPaymentUseCase : IProcessPaymentUseCase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ICalculateCartTotalUseCase _calculateCartTotalUseCase;

        public ProcessPaymentUseCase(
            IOrderRepository orderRepository,
            IPaymentGateway paymentGateway,
            ICalculateCartTotalUseCase calculateCartTotalUseCase)
        {
            _orderRepository = orderRepository;
            _paymentGateway = paymentGateway;
            _calculateCartTotalUseCase = calculateCartTotalUseCase;
        }

        public async Task ProcessPaymentAsync(ProcessPaymentInput input)
        {
            var calculateTotalInput = new CalculateCartTotalInput
            {
                CustomerId = input.UserId
            };

            decimal totalAmount = await _calculateCartTotalUseCase.CalculateTotalAsync(calculateTotalInput);

            var order = new Order(input.UserId, input.Items, totalAmount);

            await _orderRepository.CreateOrderAsync(order);

            var paymentRequest = new PaymentRequest
            {
                Amount = totalAmount,
                CardNumber = input.CardNumber,
                CardHolderName = input.CardHolderName,
                ExpirationMonthYear = input.ExpirationMonthYear,
                CVV = input.CVV,
                PostalCode = input.PostalCode
            };

            var paymentResult = await _paymentGateway.ProcessPaymentAsync(paymentRequest);

            switch (paymentResult.Status)
            {
                case PaymentStatus.Success:
                    order.Status = OrderStatus.Paid;
                    await _orderRepository.UpdateOrderAsync(order);
                    break;
                case PaymentStatus.Pending:
                    // Payment is pending, you can log a message or notify the user.
                    break;
                case PaymentStatus.Failed:
                    order.Status = OrderStatus.PaymentFailed;
                    await _orderRepository.UpdateOrderAsync(order);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}