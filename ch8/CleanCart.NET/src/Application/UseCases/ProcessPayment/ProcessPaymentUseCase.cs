using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.UseCases;
using Application.UseCases.CalculateCartTotal;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.UseCases.ProcessPayment
{
    public class ProcessPaymentUseCase : IProcessPaymentUseCase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ICalculateCartTotalUseCase _calculateCartTotalUseCase;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IMapper _mapper;

        public ProcessPaymentUseCase(
            IOrderRepository orderRepository,
            IPaymentGateway paymentGateway,
            ICalculateCartTotalUseCase calculateCartTotalUseCase,
            IShoppingCartRepository shoppingCartRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _paymentGateway = paymentGateway;
            _calculateCartTotalUseCase = calculateCartTotalUseCase;
            _shoppingCartRepository = shoppingCartRepository;
            _mapper = mapper;
        }

        public async Task<Order> ProcessPaymentAsync(ProcessPaymentInput input)
        {
            var calculateTotalInput = new CalculateCartTotalInput(input.UserId);

            decimal totalAmount = await _calculateCartTotalUseCase.CalculateTotalAsync(calculateTotalInput);

            var orderItems = _mapper.Map<List<OrderItem>>(input.Items);
            var order = new Order(input.UserId, orderItems, totalAmount);

            order = await _orderRepository.CreateOrderAsync(order);

            await _shoppingCartRepository.DeleteAsync(input.UserId);

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

            return order;
        }
    }
}