using Application.Interfaces.Data;
using Application.Operations.UseCases.AccessCustomerData;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.UnitTests.UseCases
{
    public class AccessCustomerCartQueryHandlerTests
    {
        [Theory]
        [InlineData(UserRole.CustomerService)]
        [InlineData(UserRole.Administrator)]
        public async Task GetCustomerCart_ValidInput_IsAuthorized_ReturnsCustomerCart(UserRole role)
        {
            // Arrange
            var authorizationUserId = Guid.NewGuid();
            var customerUserId = Guid.NewGuid();
            var query = new AccessCustomerCartQuery(authorizationUserId, customerUserId);
            var roles = new List<UserRole> { role };

            var mockUserRepository = new Mock<IUserQueryRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(authorizationUserId, CancellationToken.None))
                .ReturnsAsync(new User("jdoe", "jdoe@example.com", "John Does", roles));

            var mockShoppingCartRepository = new Mock<IShoppingCartQueryRepository>();
            mockShoppingCartRepository.Setup(repo => repo.GetByUserIdAsync(customerUserId))
                .ReturnsAsync(new ShoppingCart(customerUserId));

            var useCase = new AccessCustomerCartQueryHandler(mockShoppingCartRepository.Object, mockUserRepository.Object);

            // Act
            var customerCart = await useCase.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(customerCart);
        }

        [Theory]
        [InlineData(UserRole.CustomerService)]
        [InlineData(UserRole.Administrator)]
        public async Task GetOrderHistory_ValidInput_IsAuthorized_ReturnsCustomerOrderHistory(UserRole role)
        {
            // Arrange
            var authorizationUserId = Guid.NewGuid();
            var customerUserId = Guid.NewGuid();
            var query = new AccessCustomerOrderHistoryQuery(authorizationUserId, customerUserId);
            var roles = new List<UserRole> { role };

            var mockUserRepository = new Mock<IUserQueryRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(authorizationUserId, CancellationToken.None))
                .ReturnsAsync(new User("jdoe", "jdoe@example.com", "John Does", roles));

            var mockOrderRepository = new Mock<IOrderQueryRepository>();
            mockOrderRepository.Setup(repo => repo.GetOrdersByUserIdAsync(customerUserId))
                .ReturnsAsync(new List<Order>());

            var useCase = new AccessCustomerOrderHistoryQueryHandler(mockOrderRepository.Object, mockUserRepository.Object);

            // Act
            var orderHistory = await useCase.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(orderHistory);
            Assert.Empty(orderHistory);
        }

        [Fact]
        public async Task GetCustomerCart_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var authorizationUserId = Guid.NewGuid();
            var customerUserId = Guid.NewGuid();
            var query = new AccessCustomerCartQuery(authorizationUserId, customerUserId);

            var mockUserRepository = new Mock<IUserQueryRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(authorizationUserId, CancellationToken.None))
                .ReturnsAsync(new User("RegularUser", "user@example.com", "Regular User", new List<UserRole>()));

            var useCase = new AccessCustomerCartQueryHandler(Mock.Of<IShoppingCartQueryRepository>(), mockUserRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task GetOrderHistory_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var authorizationUserId = Guid.NewGuid();
            var customerUserId = Guid.NewGuid();
            var query = new AccessCustomerOrderHistoryQuery(authorizationUserId, customerUserId);

            var mockUserRepository = new Mock<IUserQueryRepository>();
            mockUserRepository.Setup(repo => repo.GetByIdAsync(authorizationUserId, CancellationToken.None))
                .ReturnsAsync(new User("RegularUser", "user@example.com", "Regular User", new List<UserRole>()));

            var useCase = new AccessCustomerOrderHistoryQueryHandler(Mock.Of<IOrderQueryRepository>(), mockUserRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.Handle(query, CancellationToken.None));
        }
    }
}
