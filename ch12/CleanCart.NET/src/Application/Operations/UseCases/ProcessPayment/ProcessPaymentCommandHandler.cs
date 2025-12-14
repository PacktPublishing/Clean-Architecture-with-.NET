using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Operations.UseCases.CalculateCartTotal;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Operations.UseCases.ProcessPayment;

public class ProcessPaymentCommandHandler(
    IOrderCommandRepository orderCommandRepository,
    IPaymentGateway paymentGateway,
    IShoppingCartCommandRepository shoppingCartCommandRepository,
    IMediator mediator,
    IMapper mapper)
    : IRequestHandler<ProcessPaymentCommand, Order>
{
    public async Task<Order> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var query = new CalculateCartTotalQuery(command.UserId);
        decimal totalAmount = await mediator.Send(query, cancellationToken);

        var orderItems = mapper.Map<List<OrderItem>>(command.Items);
        var order = new Order(command.UserId, orderItems, totalAmount);

        order = await orderCommandRepository.CreateOrderAsync(order);

        await shoppingCartCommandRepository.DeleteByUserIdAsync(command.UserId);

        var paymentRequest = new PaymentRequest
        {
            Amount = totalAmount,
            CardNumber = command.CardNumber,
            CardHolderName = command.CardHolderName,
            ExpirationMonthYear = command.ExpirationMonthYear,
            CVV = command.CVV,
            PostalCode = command.PostalCode
        };

        var paymentResult = await paymentGateway.ProcessPaymentAsync(paymentRequest);

        switch (paymentResult.Status)
        {
            case PaymentStatus.Success:
                order.Status = OrderStatus.Paid;
                await orderCommandRepository.UpdateAsync(order, cancellationToken);
                break;
            case PaymentStatus.Pending:
                // Payment is pending, you can log a message or notify the user.
                break;
            case PaymentStatus.Failed:
                order.Status = OrderStatus.PaymentFailed;
                await orderCommandRepository.UpdateAsync(order, cancellationToken);
                break;
            default:
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentOutOfRangeException(nameof(paymentResult.Status));
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
        }

        return order;
    }
}