namespace Presentation.BSA.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddPresentationLayerConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        const string envVariable = "ASPNETCORE_ENVIRONMENT";
        string environment = Environment.GetEnvironmentVariable(envVariable) ?? throw new ArgumentNullException(envVariable);

        configurationBuilder.AddJsonFile("appsettings.json", optional: false);
        configurationBuilder.AddJsonFile($"appsettings.{environment}.json", optional: false);
        configurationBuilder.AddEnvironmentVariables();

        return configurationBuilder;
    }
}