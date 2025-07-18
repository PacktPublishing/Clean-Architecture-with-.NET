using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Presentation.Common.Extensions;

namespace Presentation.IntegrationTests;

public class ConfigurationBuilderExtensionsTests
{
    private readonly IConfiguration _configuration;

    public ConfigurationBuilderExtensionsTests()
    {
        // Set test environment variable to ensure isolation
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test");

        // Mock an environment variable to ensure it overrides JSON settings and Key Vault
        Environment.SetEnvironmentVariable("AzureAd__TenantId", "env-tenant-id");

        // Create configuration builder and apply the extension method
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddPresentationLayerConfiguration();

        _configuration = configurationBuilder.Build();
    }

    [Fact]
    public void Configuration_ShouldLoad_BaseSettings()
    {
        // Arrange
        string expectedInstance = "[Enter the Login URL https://<your-tenant-name>.ciamlogin.com/]";

        // Act
        string? instanceValue = _configuration["AzureAd:Authority"];

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
        string? clientIdValue = _configuration["AzureAd:CallbackPath"];

        // Assert
        clientIdValue.Should().NotBeNullOrEmpty();
        clientIdValue.Should().Be(expectedClientId);
    }

    [Fact]
    public void KeyVault_ShouldOverride_TestConfiguration()
    {
        // Arrange
        string expectedClientId = "b7856a91-feb0-4789-adc7-7467fe779054"; // From Azure Key Vault

        // Act
        string? clientIdValue = _configuration["AzureAd:ClientId"];

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
        string? clientIdValue = _configuration["AzureAd:TenantId"];

        // Assert
        clientIdValue.Should().NotBeNullOrEmpty();
        clientIdValue.Should().Be(expectedClientId);
    }
}