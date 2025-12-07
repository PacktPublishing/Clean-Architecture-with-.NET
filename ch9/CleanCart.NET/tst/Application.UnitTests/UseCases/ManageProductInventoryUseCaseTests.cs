using Application.Interfaces.Data;
using Application.UseCases.ManageProductInventory;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.UnitTests.UseCases;

public class ManageProductInventoryUseCaseTests
{
    [Fact]
    public async Task UpdateProductInventoryAsync_AdministratorRole_UpdatesStockLevel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stockLevel = 20;

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new User("jdoe", "jdoe@example.com", "John Doe", new List<UserRole> { UserRole.Administrator }));

        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(new Product(productId, "Product Name", 10.0m, 10, ""));

        var useCase = new ManageProductInventoryUseCase(userRepository.Object, productRepository.Object);

        // Act
        await useCase.UpdateProductInventoryAsync(userId, productId, stockLevel);

        // Assert
        productRepository.Verify(repo => repo.UpdateAsync(It.Is<Product>(p => p.StockLevel == stockLevel)), Times.Once);
    }

    [Fact]
    public async Task UpdateProductInventoryAsync_InvalidRole_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stockLevel = 20;

        // Simulate a user with a non-administrator role
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new User("userName", "user@example.com", "John Doe", new List<UserRole> { UserRole.CustomerService }));

        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(new Product(productId, "Product Name", 10.0m, 10, ""));

        var useCase = new ManageProductInventoryUseCase(userRepository.Object, productRepository.Object);

        // Act and Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => useCase.UpdateProductInventoryAsync(userId, productId, stockLevel));

        // Verify that the product's stock level was not updated
        productRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }
}