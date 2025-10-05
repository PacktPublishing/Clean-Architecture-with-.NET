using System.Reflection;

namespace Domain.UnitTests.Entities;

public class ShoppingCartItemTests
{
    [Fact]
    public void ShoppingCartItem_Properties_HaveCorrectSetters()
    {
        // Arrange
        var shoppingCartItemType = typeof(ShoppingCartItem);

        // Act
        var properties = shoppingCartItemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Assert
        var filteredProperties = properties.Where(p => p.Name != nameof(ShoppingCartItem.Quantity));
        Assert.True(filteredProperties.All(p => p.GetSetMethod() == null));

        var quantityProperty = properties.Single(p => p.Name == nameof(ShoppingCartItem.Quantity));
        Assert.True(quantityProperty.GetSetMethod() != null);
    }
}
