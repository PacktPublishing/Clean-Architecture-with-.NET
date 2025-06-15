using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Operations.UseCases.CalculateCartTotal;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Operations.UseCases.ProcessPayment
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Order>
    {
        private readonly IOrderCommandRepository _orderCommandRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IMediator _mediator;
        private readonly IShoppingCartCommandRepository _shoppingCartCommandRepository;
        private readonly IMapper _mapper;

        public ProcessPaymentCommandHandler(
            IOrderCommandRepository orderCommandRepository,
            IPaymentGateway paymentGateway,
            IMediator mediator,
            IShoppingCartCommandRepository shoppingCartCommandRepository,
            IMapper mapper)
        {
            _orderCommandRepository = orderCommandRepository;
            _paymentGateway = paymentGateway;
            _mediator = mediator;
            _shoppingCartCommandRepository = shoppingCartCommandRepository;
            _mapper = mapper;
        }

        public async Task<Order> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
        {
            var query = new CalculateCartTotalQuery(command.UserId);
            decimal totalAmount = await _mediator.Send(query, cancellationToken);

            var orderItems = _mapper.Map<List<OrderItem>>(command.Items);
            var order = new Order(command.UserId, orderItems, totalAmount);

            order = await _orderCommandRepository.CreateOrderAsync(order);

            await _shoppingCartCommandRepository.DeleteByUserIdAsync(command.UserId);

            var paymentRequest = new PaymentRequest
            {
                Amount = totalAmount,
                CardNumber = command.CardNumber,
                CardHolderName = command.CardHolderName,
                ExpirationMonthYear = command.ExpirationMonthYear,
                CVV = command.CVV,
                PostalCode = command.PostalCode
            };

            var paymentResult = await _paymentGateway.ProcessPaymentAsync(paymentRequest);

            switch (paymentResult.Status)
            {
                case PaymentStatus.Success:
                    order.Status = OrderStatus.Paid;
                    await _orderCommandRepository.UpdateAsync(order, cancellationToken);
                    break;
                case PaymentStatus.Pending:
                    // Payment is pending, you can log a message or notify the user.
                    break;
                case PaymentStatus.Failed:
                    order.Status = OrderStatus.PaymentFailed;
                    await _orderCommandRepository.UpdateAsync(order, cancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return order;
        }
    }
}