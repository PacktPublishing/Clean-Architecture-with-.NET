using System.Reflection;

namespace Domain.UnitTests.Entities;

public class ProductTests
{
    [Fact]
    public void Product_Properties_HavePrivateSetters()
    {
        var productType = typeof(Product);

        var properties = productType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Assert.True(properties.All(p => p.GetSetMethod() == null));
    }
}