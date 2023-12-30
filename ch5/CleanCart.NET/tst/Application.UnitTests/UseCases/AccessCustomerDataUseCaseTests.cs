using Application.Interfaces.Data;
using Application.UseCases.AccessCustomerData;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.UnitTests.UseCases
{
    public class AccessCustomerDataUseCaseTests
    {
        [Theory]
        [InlineData(UserRole.CustomerService)]
        [InlineData(UserRole.Administrator)]
        public async Task GetCustomerCart_ValidInput_IsAuthorized_ReturnsCustomerCart(UserRole role)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var roles = new List<UserRole> { role };

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User("jdoe", "jdoe@example.com", "John Does", roles));

            var mockShoppingCartRepository = new Mock<IShoppingCartRepository>();
            mockShoppingCartRepository.Setup(repo => repo.GetByCustomerIdAsync(customerId))
                .ReturnsAsync(new ShoppingCart(customerId));

            var useCase = new AccessCustomerDataUseCase(
                Mock.Of<IOrderRepository>(),
                mockShoppingCartRepository.Object,
                mockUserRepository.Object);

            // Act
            var customerCart = await useCase.GetCustomerCartAsync(userId, customerId);

            // Assert
            Assert.NotNull(customerCart);
        }

        [Theory]
        [InlineData(UserRole.CustomerService)]
        [InlineData(UserRole.Administrator)]
        public async Task GetOrderHistory_ValidInput_IsAuthorized_ReturnsCustomerOrderHistory(UserRole role)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var roles = new List<UserRole> { role };

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User("jdoe", "jdoe@example.com", "John Does", roles));

            var mockOrderRepository = new Mock<IOrderRepository>();
            mockOrderRepository.Setup(repo => repo.GetOrdersByUserIdAsync(customerId))
                .ReturnsAsync(new List<Order>());

            var useCase = new AccessCustomerDataUseCase(
                mockOrderRepository.Object,
                Mock.Of<IShoppingCartRepository>(),
                mockUserRepository.Object);

            // Act
            var orderHistory = await useCase.GetOrderHistoryAsync(userId, customerId);

            // Assert
            Assert.NotNull(orderHistory);
            Assert.Empty(orderHistory);
        }

        [Fact]
        public async Task GetCustomerCart_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User("RegularUser", "user@example.com", "Regular User", new List<UserRole>()));

            var useCase = new AccessCustomerDataUseCase(
                Mock.Of<IOrderRepository>(),
                Mock.Of<IShoppingCartRepository>(),
                mockUserRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.GetCustomerCartAsync(userId, customerId));
        }

        [Fact]
        public async Task GetOrderHistory_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User("RegularUser", "user@example.com", "Regular User", new List<UserRole>()));

            var useCase = new AccessCustomerDataUseCase(
                Mock.Of<IOrderRepository>(),
                Mock.Of<IShoppingCartRepository>(),
                mockUserRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.GetOrderHistoryAsync(userId, customerId));
        }
    }
}
