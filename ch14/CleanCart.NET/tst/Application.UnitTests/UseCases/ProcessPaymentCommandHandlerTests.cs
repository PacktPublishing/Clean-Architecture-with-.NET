using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Operations.UseCases.CalculateCartTotal;
using Application.Operations.UseCases.ProcessPayment;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Moq;

namespace Application.UnitTests.UseCases
{
    public class ProcessPaymentCommandHandlerTests
    {
        [Fact]
        public async Task ProcessPaymentAsync_PaymentSuccessful_UpdatesOrderStatusToPaid()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(mapper => mapper.Map<List<OrderItem>>(It.IsAny<List<ShoppingCartItem>>()))
                .Returns((List<ShoppingCartItem> items) => items.ConvertAll(item => new OrderItem(item.ProductId, item.ProductName, item.ProductPrice, item.Quantity)));

            var mockOrderRepository = new Mock<IOrderCommandRepository>();
            mockOrderRepository.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order order) => order); // Return the order passed to it

            var mockShoppingCartRepository = new Mock<IShoppingCartCommandRepository>();

            var mockPaymentGateway = new Mock<IPaymentGateway>();
            mockPaymentGateway.Setup(gateway => gateway.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ReturnsAsync(new PaymentResult { Status = PaymentStatus.Success });

            var mockCalculateCartTotalUseCase = new Mock<IMediator>();
            mockCalculateCartTotalUseCase.Setup(useCase => useCase.Send(It.IsAny<CalculateCartTotalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(100.0m);

            var useCase = new ProcessPaymentCommandHandler(
                mockOrderRepository.Object,
                mockPaymentGateway.Object,
                mockCalculateCartTotalUseCase.Object,
                mockShoppingCartRepository.Object,
                mockMapper.Object);

            var command = new ProcessPaymentCommand
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
            await useCase.Handle(command, CancellationToken.None);

            // Assert
            mockOrderRepository.Verify(repo => repo.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Paid), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentAsync_PaymentFailed_UpdatesOrderStatusToPaymentFailed()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(mapper => mapper.Map<List<OrderItem>>(It.IsAny<List<ShoppingCartItem>>()))
                .Returns((List<ShoppingCartItem> items) => items.ConvertAll(item => new OrderItem(item.ProductId, item.ProductName, item.ProductPrice, item.Quantity)));

            var mockOrderRepository = new Mock<IOrderCommandRepository>();
            mockOrderRepository.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order order) => order); // Return the order passed to it

            var mockShoppingCartRepository = new Mock<IShoppingCartCommandRepository>();

            var mockPaymentGateway = new Mock<IPaymentGateway>();
            mockPaymentGateway.Setup(gateway => gateway.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ReturnsAsync(new PaymentResult { Status = PaymentStatus.Failed });

            var mockCalculateCartTotalUseCase = new Mock<IMediator>();
            mockCalculateCartTotalUseCase.Setup(useCase => useCase.Send(It.IsAny<CalculateCartTotalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(100.0m);

            var useCase = new ProcessPaymentCommandHandler(
                mockOrderRepository.Object,
                mockPaymentGateway.Object,
                mockCalculateCartTotalUseCase.Object,
                mockShoppingCartRepository.Object,
                mockMapper.Object);

            var command = new ProcessPaymentCommand
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
            await useCase.Handle(command, CancellationToken.None);

            // Assert
            mockOrderRepository.Verify(repo => repo.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.PaymentFailed), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
