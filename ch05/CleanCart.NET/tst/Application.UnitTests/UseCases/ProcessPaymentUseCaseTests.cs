using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.UseCases;
using Application.UseCases.CalculateCartTotal;
using Application.UseCases.ProcessPayment;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class ProcessPaymentUseCaseTests
{
    [Fact]
    public async Task ProcessPaymentAsync_PaymentSuccessful_UpdatesOrderStatusToPaid()
    {
        // Arrange
        var mockOrderRepository = Substitute.For<IOrderRepository>();
        mockOrderRepository.CreateOrderAsync(Arg.Any<Order>())
            .Returns(callInfo => callInfo.Arg<Order>()); // Return the order passed to it

        var mockPaymentGateway = Substitute.For<IPaymentGateway>();
        mockPaymentGateway.ProcessPaymentAsync(Arg.Any<PaymentRequest>())
            .Returns(new PaymentResult { Status = PaymentStatus.Success });

        var shoppingCartRepository = Substitute.For<IShoppingCartRepository>();

        var mockCalculateCartTotalUseCase = Substitute.For<ICalculateCartTotalUseCase>();
        mockCalculateCartTotalUseCase.CalculateTotalAsync(Arg.Any<CalculateCartTotalInput>())
            .Returns(100.0m);

        var useCase = new ProcessPaymentUseCase(
            mockOrderRepository,
            mockPaymentGateway,
            shoppingCartRepository,
            mockCalculateCartTotalUseCase);

        var input = new ProcessPaymentInput
        {
            UserId = Guid.NewGuid(),
            Items = new List<ShoppingCartItem>(),
            CardNumber = "1234567890123456",
            CardHolderName = "John Doe",
            ExpirationMonthYear = "12/23",
            CVV = "123",
            PostalCode = "12345"
        };

        // Act
        await useCase.ProcessPaymentAsync(input);

        // Assert
        await mockOrderRepository.Received(1).UpdateOrderAsync(Arg.Is<Order>(o => o.Status == OrderStatus.Paid));
    }

    [Fact]
    public async Task ProcessPaymentAsync_PaymentFailed_UpdatesOrderStatusToPaymentFailed()
    {
        // Arrange
        var mockOrderRepository = Substitute.For<IOrderRepository>();
        mockOrderRepository.CreateOrderAsync(Arg.Any<Order>())
            .Returns(callInfo => callInfo.Arg<Order>()); // Return the order passed to it

        var mockPaymentGateway = Substitute.For<IPaymentGateway>();
        mockPaymentGateway.ProcessPaymentAsync(Arg.Any<PaymentRequest>())
            .Returns(new PaymentResult { Status = PaymentStatus.Failed });

        var shoppingCartRepository = Substitute.For<IShoppingCartRepository>();

        var mockCalculateCartTotalUseCase = Substitute.For<ICalculateCartTotalUseCase>();
        mockCalculateCartTotalUseCase.CalculateTotalAsync(Arg.Any<CalculateCartTotalInput>())
            .Returns(100.0m);

        var useCase = new ProcessPaymentUseCase(
            mockOrderRepository,
            mockPaymentGateway,
            shoppingCartRepository,
            mockCalculateCartTotalUseCase);

        var input = new ProcessPaymentInput
        {
            UserId = Guid.NewGuid(),
            Items = new List<ShoppingCartItem>(),
            CardNumber = "1234567890123456",
            CardHolderName = "John Doe",
            ExpirationMonthYear = "12/23",
            CVV = "123",
            PostalCode = "12345"
        };

        // Act
        await useCase.ProcessPaymentAsync(input);

        // Assert
        await mockOrderRepository.Received(1).UpdateOrderAsync(Arg.Is<Order>(o => o.Status == OrderStatus.PaymentFailed));
    }
}
