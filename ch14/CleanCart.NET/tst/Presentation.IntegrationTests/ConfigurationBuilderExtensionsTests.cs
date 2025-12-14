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
        string expectedInstance = "[Enter the Login URL https://<your-tenant-name>.ciamlogin.com/]";

        string? instanceValue = _configuration["AzureAd:Authority"];

        instanceValue.Should().NotBeNullOrEmpty();
        instanceValue.Should().Be(expectedInstance);
    }

    [Fact]
    public void Configuration_ShouldLoad_TestEnvironmentOverrides()
    {
        string expectedClientId = "test-callback-path"; // From appsettings.test.json

        string? clientIdValue = _configuration["AzureAd:CallbackPath"];

        clientIdValue.Should().NotBeNullOrEmpty();
        clientIdValue.Should().Be(expectedClientId);
    }

    [Fact]
    public void KeyVault_ShouldOverride_TestConfiguration()
    {
        string expectedClientId = "b7856a91-feb0-4789-adc7-7467fe779054"; // From Azure Key Vault

        string? clientIdValue = _configuration["AzureAd:ClientId"];

        clientIdValue.Should().NotBeNullOrEmpty();
        clientIdValue.Should().Be(expectedClientId);
    }

    [Fact]
    public void EnvironmentVariables_ShouldOverride_AllOtherSources()
    {
        string expectedClientId = "env-tenant-id"; // From environment variable

        string? clientIdValue = _configuration["AzureAd:TenantId"];

        clientIdValue.Should().NotBeNullOrEmpty();
        clientIdValue.Should().Be(expectedClientId);
    }
}