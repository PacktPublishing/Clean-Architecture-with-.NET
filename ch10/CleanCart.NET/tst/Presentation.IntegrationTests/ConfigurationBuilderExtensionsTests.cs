using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Presentation.BSA.Extensions;

namespace Presentation.IntegrationTests;

public class ConfigurationBuilderExtensionsTests
{
    private readonly IConfiguration _configuration;

    public ConfigurationBuilderExtensionsTests()
    {
        // Set test environment variable to ensure isolation
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test");

        // Mock an environment variable to ensure it overrides JSON settings and Key Vault
        Environment.SetEnvironmentVariable("AzureADB2C__TenantId", "env-tenant-id");

        // Create configuration builder and apply the extension method
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddPresentationLayerConfiguration();

        _configuration = configurationBuilder.Build();
    }

    [Fact]
    public void Configuration_ShouldLoad_BaseSettings()
    {
        // Arrange
        string expectedInstance = "https://--your-domain--.b2clogin.com";

        // Act
        string? instanceValue = _configuration["AzureADB2C:Instance"];

        // Assert
        instanceValue.Should().NotBeNullOrEmpty();
        instanceValue.Should().Be(expectedInstance);
    }

    [Fact]
    public void Configuration_ShouldLoad_TestEnvironmentOverrides()
    {
        // Arrange
        string expectedClientId = "test-callback-path"; // From appsettings.test.json

        // Act
        string? clientIdValue = _configuration["AzureADB2C:CallbackPath"];

        // Assert
        clientIdValue.Should().NotBeNullOrEmpty();
        clientIdValue.Should().Be(expectedClientId);
    }

    [Fact]
    public void KeyVault_ShouldOverride_TestConfiguration()
    {
        // Arrange
        string expectedClientId = "dd5e41bf-bbe2-4801-9022-f87041da5893"; // From Azure Key Vault

        // Act
        string? clientIdValue = _configuration["AzureADB2C:ClientId"];

        // Assert
        clientIdValue.Should().NotBeNullOrEmpty();
        clientIdValue.Should().Be(expectedClientId);
    }

    [Fact]
    public void EnvironmentVariables_ShouldOverride_AllOtherSources()
    {
        // Arrange
        string expectedClientId = "env-tenant-id"; // From environment variable

        // Act
        string? clientIdValue = _configuration["AzureADB2C:TenantId"];

        // Assert
        clientIdValue.Should().NotBeNullOrEmpty();
        clientIdValue.Should().Be(expectedClientId);
    }
}