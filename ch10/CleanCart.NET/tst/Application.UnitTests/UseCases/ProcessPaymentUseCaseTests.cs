using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.UseCases;
using Application.Mapping;
using Application.UseCases.CalculateCartTotal;
using Application.UseCases.ProcessPayment;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class ProcessPaymentUseCaseTests
{
    private readonly IMapper _mapper;

    public ProcessPaymentUseCaseTests()
    {
        _mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ApplicationMappingProfile>();
        }).CreateMapper();
    }

    [Fact]
    public async Task ProcessPaymentAsync_PaymentSuccessful_UpdatesOrderStatusToPaid()
    {
        var mockOrderRepository = Substitute.For<IOrderRepository>();
        mockOrderRepository.CreateOrderAsync(Arg.Any<Order>())
            .Returns(info => info.Arg<Order>()); // Return the order passed to it

        var mockShoppingCartRepository = Substitute.For<IShoppingCartRepository>();
        mockShoppingCartRepository.GetByUserIdAsync(Arg.Any<Guid>())
            .Returns(new ShoppingCart(Guid.NewGuid()));

        var mockPaymentGateway = Substitute.For<IPaymentGateway>();
        mockPaymentGateway.ProcessPaymentAsync(Arg.Any<PaymentRequest>())
            .Returns(new PaymentResult { Status = PaymentStatus.Success });

        var mockCalculateCartTotalUseCase = Substitute.For<ICalculateCartTotalUseCase>();
        mockCalculateCartTotalUseCase.CalculateTotalAsync(Arg.Any<CalculateCartTotalInput>())
            .Returns(100.0m);

        var useCase = new ProcessPaymentUseCase(
            mockOrderRepository,
            mockPaymentGateway,
            mockShoppingCartRepository,
            mockCalculateCartTotalUseCase,
            _mapper);

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

        await useCase.ProcessPaymentAsync(input);

        await mockShoppingCartRepository.Received(1).DeleteByUserIdAsync(Arg.Any<Guid>());
        await mockOrderRepository.Received(1).UpdateOrderAsync(Arg.Is<Order>(o => o.Status == OrderStatus.Paid));
    }

    [Fact]
    public async Task ProcessPaymentAsync_PaymentFailed_UpdatesOrderStatusToPaymentFailed()
    {
        var mockOrderRepository = Substitute.For<IOrderRepository>();
        mockOrderRepository.CreateOrderAsync(Arg.Any<Order>())
            .Returns(info => info.Arg<Order>()); // Return the order passed to it

        var mockShoppingCartRepository = Substitute.For<IShoppingCartRepository>();
        mockShoppingCartRepository.GetByUserIdAsync(Arg.Any<Guid>())
            .Returns(new ShoppingCart(Guid.NewGuid()));

        var mockPaymentGateway = Substitute.For<IPaymentGateway>();
        mockPaymentGateway.ProcessPaymentAsync(Arg.Any<PaymentRequest>())
            .Returns(new PaymentResult { Status = PaymentStatus.Failed });

        var mockCalculateCartTotalUseCase = Substitute.For<ICalculateCartTotalUseCase>();
        mockCalculateCartTotalUseCase.CalculateTotalAsync(Arg.Any<CalculateCartTotalInput>())
            .Returns(100.0m);

        var useCase = new ProcessPaymentUseCase(
            mockOrderRepository,
            mockPaymentGateway,
            mockShoppingCartRepository,
            mockCalculateCartTotalUseCase,
            _mapper);

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

        await useCase.ProcessPaymentAsync(input);

        await mockShoppingCartRepository.Received(1).DeleteByUserIdAsync(Arg.Any<Guid>());
        await mockOrderRepository.Received(1).UpdateOrderAsync(Arg.Is<Order>(o => o.Status == OrderStatus.PaymentFailed));
    }
}