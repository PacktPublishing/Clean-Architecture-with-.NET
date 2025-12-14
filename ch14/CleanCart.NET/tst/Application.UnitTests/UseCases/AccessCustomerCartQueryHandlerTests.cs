using Application.Interfaces.Data;
using Application.Operations.UseCases.AccessCustomerData;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class AccessCustomerCartQueryHandlerTests
{
    [Theory]
    [InlineData(UserRole.CustomerService)]
    [InlineData(UserRole.Administrator)]
    public async Task GetCustomerCart_ValidInput_IsAuthorized_ReturnsCustomerCart(UserRole role)
    {
        var authorizationUserId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();
        var query = new AccessCustomerCartQuery(authorizationUserId, customerUserId);
        var roles = new List<UserRole> { role };

        var mockUserRepository = Substitute.For<IUserQueryRepository>();
        mockUserRepository.GetByIdAsync(authorizationUserId, CancellationToken.None)
            .Returns(new User("jdoe", "jdoe@example.com", "John Does", roles));

        var mockShoppingCartRepository = Substitute.For<IShoppingCartQueryRepository>();
        mockShoppingCartRepository.GetByUserIdAsync(customerUserId)
            .Returns(new ShoppingCart(customerUserId));

        var useCase = new AccessCustomerCartQueryHandler(mockShoppingCartRepository, mockUserRepository);

        var customerCart = await useCase.Handle(query, CancellationToken.None);

        Assert.NotNull(customerCart);
    }

    [Theory]
    [InlineData(UserRole.CustomerService)]
    [InlineData(UserRole.Administrator)]
    public async Task GetOrderHistory_ValidInput_IsAuthorized_ReturnsCustomerOrderHistory(UserRole role)
    {
        var authorizationUserId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();
        var query = new AccessCustomerOrderHistoryQuery(authorizationUserId, customerUserId);
        var roles = new List<UserRole> { role };

        var mockUserRepository = Substitute.For<IUserQueryRepository>();
        mockUserRepository.GetByIdAsync(authorizationUserId, CancellationToken.None)
            .Returns(new User("jdoe", "jdoe@example.com", "John Does", roles));

        var mockOrderRepository = Substitute.For<IOrderQueryRepository>();
        mockOrderRepository.GetOrdersByUserIdAsync(customerUserId, CancellationToken.None)
            .Returns(new List<Order>());

        var useCase = new AccessCustomerOrderHistoryQueryHandler(mockOrderRepository, mockUserRepository);

        var orderHistory = await useCase.Handle(query, CancellationToken.None);

        Assert.NotNull(orderHistory);
        Assert.Empty(orderHistory);
    }

    [Fact]
    public async Task GetCustomerCart_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        var authorizationUserId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();
        var query = new AccessCustomerCartQuery(authorizationUserId, customerUserId);

        var mockUserRepository = Substitute.For<IUserQueryRepository>();
        mockUserRepository.GetByIdAsync(authorizationUserId, CancellationToken.None)
            .Returns(new User("RegularUser", "user@example.com", "Regular User", new List<UserRole>()));

        var useCase = new AccessCustomerCartQueryHandler(Substitute.For<IShoppingCartQueryRepository>(), mockUserRepository);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task GetOrderHistory_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        var authorizationUserId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();
        var query = new AccessCustomerOrderHistoryQuery(authorizationUserId, customerUserId);

        var mockUserRepository = Substitute.For<IUserQueryRepository>();
        mockUserRepository.GetByIdAsync(authorizationUserId, CancellationToken.None)
            .Returns(new User("RegularUser", "user@example.com", "Regular User", new List<UserRole>()));

        var useCase = new AccessCustomerOrderHistoryQueryHandler(Substitute.For<IOrderQueryRepository>(), mockUserRepository);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.Handle(query, CancellationToken.None));
    }
}