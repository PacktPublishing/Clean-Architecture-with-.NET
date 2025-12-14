using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Mapping;
using Application.Operations.UseCases.CalculateCartTotal;
using Application.Operations.UseCases.ProcessPayment;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class ProcessPaymentCommandHandlerTests
{
    private readonly IMapper _mapper;

    public ProcessPaymentCommandHandlerTests()
    {
        _mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ApplicationMappingProfile>();
        }).CreateMapper();
    }

    [Fact]
    public async Task ProcessPaymentCommandHandler_PaymentSuccessful_UpdatesOrderStatusToPaid()
    {
        var mockOrderRepository = Substitute.For<IOrderCommandRepository>();
        mockOrderRepository.CreateOrderAsync(Arg.Any<Order>())
            .Returns(info => info.Arg<Order>()); // Return the order passed to it

        var mockShoppingCartRepository = Substitute.For<IShoppingCartCommandRepository>();

        var mockPaymentGateway = Substitute.For<IPaymentGateway>();
        mockPaymentGateway.ProcessPaymentAsync(Arg.Any<PaymentRequest>())
            .Returns(new PaymentResult { Status = PaymentStatus.Success });

        var mockCalculateCartTotalUseCase = Substitute.For<IMediator>();
        mockCalculateCartTotalUseCase.Send(Arg.Any<CalculateCartTotalQuery>(), Arg.Any<CancellationToken>())
            .Returns(100.0m);

        var useCase = new ProcessPaymentCommandHandler(
            mockOrderRepository,
            mockPaymentGateway,
            mockShoppingCartRepository,
            mockCalculateCartTotalUseCase,
            _mapper);

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

        await useCase.Handle(command, CancellationToken.None);

        await mockShoppingCartRepository.Received(1).DeleteByUserIdAsync(Arg.Any<Guid>());
        await mockOrderRepository.Received(1).UpdateAsync(Arg.Is<Order>(o => o.Status == OrderStatus.Paid), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPaymentCommandHandler_PaymentFailed_UpdatesOrderStatusToPaymentFailed()
    {
        var mockOrderRepository = Substitute.For<IOrderCommandRepository>();
        mockOrderRepository.CreateOrderAsync(Arg.Any<Order>())
            .Returns(info => info.Arg<Order>()); // Return the order passed to it

        var mockShoppingCartRepository = Substitute.For<IShoppingCartCommandRepository>();

        var mockPaymentGateway = Substitute.For<IPaymentGateway>();
        mockPaymentGateway.ProcessPaymentAsync(Arg.Any<PaymentRequest>())
            .Returns(new PaymentResult { Status = PaymentStatus.Failed });

        var mockCalculateCartTotalUseCase = Substitute.For<IMediator>();
        mockCalculateCartTotalUseCase.Send(Arg.Any<CalculateCartTotalQuery>(), Arg.Any<CancellationToken>())
            .Returns(100.0m);

        var useCase = new ProcessPaymentCommandHandler(
            mockOrderRepository,
            mockPaymentGateway,
            mockShoppingCartRepository,
            mockCalculateCartTotalUseCase,
            _mapper);

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

        await useCase.Handle(command, CancellationToken.None);

        await mockShoppingCartRepository.Received(1).DeleteByUserIdAsync(Arg.Any<Guid>());
        await mockOrderRepository.Received(1).UpdateAsync(Arg.Is<Order>(o => o.Status == OrderStatus.PaymentFailed), Arg.Any<CancellationToken>());
    }
}