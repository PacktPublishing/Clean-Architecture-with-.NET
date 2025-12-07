using Application.Interfaces.Data;
using Application.Operations.UseCases.ManageProductInventory;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.UnitTests.UseCases;

public class UpdateProductInventoryCommandHandlerTests
{
    [Fact]
    public async Task UpdateProductInventoryAsync_AdministratorRole_UpdatesStockLevel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stockLevel = 20;
        var command = new UpdateProductInventoryCommand(userId, productId, stockLevel);

        var userRepository = new Mock<IUserQueryRepository>();
        userRepository.Setup(repo => repo.GetByIdAsync(userId, CancellationToken.None))
            .ReturnsAsync(new User("jdoe", "jdoe@example.com", "John Doe", new List<UserRole> { UserRole.Administrator }));

        var productQueryRepository = new Mock<IProductQueryRepository>();
        productQueryRepository.Setup(repo => repo.GetByIdAsync(productId, CancellationToken.None))
            .ReturnsAsync(new Product(productId, "Product Name", 10.0m, 10, ""));

        var productCommandRepository = new Mock<IProductCommandRepository>();

        var useCase = new UpdateProductInventoryCommandHandler(userRepository.Object, productQueryRepository.Object, productCommandRepository.Object);

        // Act
        await useCase.Handle(command, CancellationToken.None);

        // Assert
        productCommandRepository.Verify(repo => repo.UpdateAsync(It.Is<Product>(p => p.StockLevel == stockLevel), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductInventoryAsync_InvalidRole_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stockLevel = 20;
        var command = new UpdateProductInventoryCommand(userId, productId, stockLevel);

        // Simulate a user with a non-administrator role
        var userRepository = new Mock<IUserQueryRepository>();
        userRepository.Setup(repo => repo.GetByIdAsync(userId, CancellationToken.None))
            .ReturnsAsync(new User("userName", "user@example.com", "John Doe", new List<UserRole> { UserRole.CustomerService }));

        var productQueryRepository = new Mock<IProductQueryRepository>();
        productQueryRepository.Setup(repo => repo.GetByIdAsync(productId, CancellationToken.None))
            .ReturnsAsync(new Product(productId, "Product Name", 10.0m, 10, ""));

        var productCommandRepository = new Mock<IProductCommandRepository>();

        var useCase = new UpdateProductInventoryCommandHandler(userRepository.Object, productQueryRepository.Object, productCommandRepository.Object);

        // Act and Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.Handle(command, CancellationToken.None));

        // Verify that the product's stock level was not updated
        productCommandRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}