using System.Reflection;

namespace Domain.UnitTests.Entities;

public class ShoppingCartItemTests
{
    [Fact]
    public void ShoppingCartItem_Properties_HaveCorrectSetters()
    {
        var shoppingCartItemType = typeof(ShoppingCartItem);

        var properties = shoppingCartItemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var filteredProperties = properties.Where(p => p.Name != nameof(ShoppingCartItem.Quantity));
        Assert.True(filteredProperties.All(p => p.GetSetMethod() == null));

        var quantityProperty = properties.Single(p => p.Name == nameof(ShoppingCartItem.Quantity));
        Assert.True(quantityProperty.GetSetMethod() != null);
    }
}