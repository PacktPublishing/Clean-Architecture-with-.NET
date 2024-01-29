using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.UseCases;
using Application.UseCases.CalculateCartTotal;
using Application.UseCases.ProcessPayment;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.UnitTests.UseCases
{
    public class ProcessPaymentUseCaseTests
    {
        [Fact]
        public async Task ProcessPayment_PaymentSuccessful_UpdatesOrderStatusToPaid()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(mapper => mapper.Map<List<OrderItem>>(It.IsAny<List<ShoppingCartItem>>()))
                .Returns((List<ShoppingCartItem> items) => items.ConvertAll(item => new OrderItem(item.ProductId, item.ProductName, item.ProductPrice, item.Quantity)));

            var mockOrderRepository = new Mock<IOrderRepository>();
            mockOrderRepository.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order order) => order); // Return the order passed to it

            var mockPaymentGateway = new Mock<IPaymentGateway>();
            mockPaymentGateway.Setup(gateway => gateway.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ReturnsAsync(new PaymentResult { Status = PaymentStatus.Success });

            var mockCalculateCartTotalUseCase = new Mock<ICalculateCartTotalUseCase>();
            mockCalculateCartTotalUseCase.Setup(useCase => useCase.CalculateTotal(It.IsAny<CalculateCartTotalInput>()))
                .ReturnsAsync(100.0m);

            var useCase = new ProcessPaymentUseCase(
                mockOrderRepository.Object,
                mockPaymentGateway.Object,
                mockCalculateCartTotalUseCase.Object,
                mockMapper.Object);

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
            await useCase.ProcessPayment(input);

            // Assert
            mockOrderRepository.Verify(repo => repo.UpdateOrderAsync(It.Is<Order>(o => o.Status == OrderStatus.Paid)), Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_PaymentFailed_UpdatesOrderStatusToPaymentFailed()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(mapper => mapper.Map<List<OrderItem>>(It.IsAny<List<ShoppingCartItem>>()))
                .Returns((List<ShoppingCartItem> items) => items.ConvertAll(item => new OrderItem(item.ProductId, item.ProductName, item.ProductPrice, item.Quantity)));

            var mockOrderRepository = new Mock<IOrderRepository>();
            mockOrderRepository.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order order) => order); // Return the order passed to it

            var mockPaymentGateway = new Mock<IPaymentGateway>();
            mockPaymentGateway.Setup(gateway => gateway.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ReturnsAsync(new PaymentResult { Status = PaymentStatus.Failed });

            var mockCalculateCartTotalUseCase = new Mock<ICalculateCartTotalUseCase>();
            mockCalculateCartTotalUseCase.Setup(useCase => useCase.CalculateTotal(It.IsAny<CalculateCartTotalInput>()))
                .ReturnsAsync(100.0m);

            var useCase = new ProcessPaymentUseCase(
                mockOrderRepository.Object,
                mockPaymentGateway.Object,
                mockCalculateCartTotalUseCase.Object,
                mockMapper.Object);

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
            await useCase.ProcessPayment(input);

            // Assert
            mockOrderRepository.Verify(repo => repo.UpdateOrderAsync(It.Is<Order>(o => o.Status == OrderStatus.PaymentFailed)), Times.Once);
        }
    }
}
