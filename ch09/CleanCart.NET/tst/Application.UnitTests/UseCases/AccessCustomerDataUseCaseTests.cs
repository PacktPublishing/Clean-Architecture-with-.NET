using Application.Interfaces.Data;
using Application.UseCases.AccessCustomerData;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Application.UnitTests.UseCases;

public class AccessCustomerDataUseCaseTests
{
    [Theory]
    [InlineData(UserRole.CustomerService)]
    [InlineData(UserRole.Administrator)]
    public async Task GetCustomerCart_ValidInput_IsAuthorized_ReturnsCustomerCart(UserRole role)
    {
        var authorizationUserId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();
        var roles = new List<UserRole> { role };

        var mockUserRepository = Substitute.For<IUserRepository>();
        mockUserRepository.GetByIdAsync(authorizationUserId)
            .Returns(new User("jdoe", "jdoe@example.com", "John Does", roles));

        var mockShoppingCartRepository = Substitute.For<IShoppingCartRepository>();
        mockShoppingCartRepository.GetByUserIdAsync(customerUserId)
            .Returns(new ShoppingCart(customerUserId));

        var useCase = new AccessCustomerDataUseCase(
            Substitute.For<IOrderRepository>(),
            mockShoppingCartRepository,
            mockUserRepository);

        var customerCart = await useCase.GetCustomerCartAsync(authorizationUserId, customerUserId);

        Assert.NotNull(customerCart);
    }

    [Theory]
    [InlineData(UserRole.CustomerService)]
    [InlineData(UserRole.Administrator)]
    public async Task GetOrderHistory_ValidInput_IsAuthorized_ReturnsCustomerOrderHistory(UserRole role)
    {
        var authorizationUserId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();
        var roles = new List<UserRole> { role };

        var mockUserRepository = Substitute.For<IUserRepository>();
        mockUserRepository.GetByIdAsync(authorizationUserId)
            .Returns(new User("jdoe", "jdoe@example.com", "John Does", roles));

        var mockOrderRepository = Substitute.For<IOrderRepository>();
        mockOrderRepository.GetOrdersByUserIdAsync(customerUserId)
            .Returns(new List<Order>());

        var useCase = new AccessCustomerDataUseCase(
            mockOrderRepository,
            Substitute.For<IShoppingCartRepository>(),
            mockUserRepository);

        var orderHistory = await useCase.GetOrderHistoryAsync(authorizationUserId, customerUserId);

        Assert.NotNull(orderHistory);
        Assert.Empty(orderHistory);
    }

    [Fact]
    public async Task GetCustomerCart_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        var authorizationUserId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();

        var mockUserRepository = Substitute.For<IUserRepository>();
        mockUserRepository.GetByIdAsync(authorizationUserId)
            .Returns(new User("RegularUser", "user@example.com", "Regular User", new List<UserRole>()));

        var useCase = new AccessCustomerDataUseCase(
            Substitute.For<IOrderRepository>(),
            Substitute.For<IShoppingCartRepository>(),
            mockUserRepository);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.GetCustomerCartAsync(authorizationUserId, customerUserId));
    }

    [Fact]
    public async Task GetOrderHistory_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        var authorizationUserId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();

        var mockUserRepository = Substitute.For<IUserRepository>();
        mockUserRepository.GetByIdAsync(authorizationUserId)
            .Returns(new User("RegularUser", "user@example.com", "Regular User", new List<UserRole>()));

        var useCase = new AccessCustomerDataUseCase(
            Substitute.For<IOrderRepository>(),
            Substitute.For<IShoppingCartRepository>(),
            mockUserRepository);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.GetOrderHistoryAsync(authorizationUserId, customerUserId));
    }
}