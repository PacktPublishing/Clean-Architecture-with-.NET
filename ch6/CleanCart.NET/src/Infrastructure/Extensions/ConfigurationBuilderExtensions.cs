using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddCoreLayerConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        const string envVariable = "ASPNETCORE_ENVIRONMENT";
        string environment = Environment.GetEnvironmentVariable(envVariable) ?? throw new ArgumentNullException(envVariable);

        configurationBuilder.AddJsonFile("appsettings.core.json", optional: false);
        configurationBuilder.AddJsonFile($"appsettings.core.{environment}.json", optional: false);
        configurationBuilder.AddEnvironmentVariables();

        return configurationBuilder;
    }
}