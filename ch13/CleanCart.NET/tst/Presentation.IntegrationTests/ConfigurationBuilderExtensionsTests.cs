using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Presentation.BSA.Extensions;

namespace Presentation.IntegrationTests;

public class ConfigurationBuilderExtensionsTests
{
    private readonly IConfiguration _configuration;

    public ConfigurationBuilderExtensionsTests()
    {
        // Ensure test environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test");

        // Environment variable should override all other configuration sources
        Environment.SetEnvironmentVariable("IntegrationTest__EnvVarOverride", "env-value");

        var baseValues = new Dictionary<string, string?>
        {
            ["IntegrationTest:BaseOnly"] = "base-value"
        };

        var configurationBuilder = new ConfigurationBuilder();

        // Base configuration
        configurationBuilder.AddInMemoryCollection(baseValues);

        // Application configuration pipeline
        configurationBuilder.AddPresentationLayerConfiguration();

        _configuration = configurationBuilder.Build();
    }

    [Fact]
    public void BaseConfiguration_ShouldLoad_BaseValue()
    {
        string expectedValue = "base-value";

        string? value = _configuration["IntegrationTest:BaseOnly"];

        value.Should().NotBeNullOrEmpty();
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void EnvironmentJson_ShouldOverride_BaseConfiguration()
    {
        string expectedValue = "json-env-value";

        string? value = _configuration["IntegrationTest:EnvOverride"];

        value.Should().NotBeNullOrEmpty();
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void KeyVault_ShouldOverride_JsonConfiguration()
    {
        string expectedValue = "keyvault-value";

        string? value = _configuration["IntegrationTest:KeyVaultOverride"];

        value.Should().NotBeNullOrEmpty();
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void EnvironmentVariables_ShouldOverride_AllOtherSources()
    {
        string expectedValue = "env-value";

        string? value = _configuration["IntegrationTest:EnvVarOverride"];

        value.Should().NotBeNullOrEmpty();
        value.Should().Be(expectedValue);
    }
}