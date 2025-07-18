using Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddCoreLayerConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        string environment = AspNetEnvironmentHelper.GetRequiredEnvironmentName();
        configurationBuilder.AddJsonFile("appsettings.core.json", optional: false);
        configurationBuilder.AddJsonFile($"appsettings.core.{environment}.json", optional: false);
        configurationBuilder.AddEnvironmentVariables();

        return configurationBuilder;
    }
}