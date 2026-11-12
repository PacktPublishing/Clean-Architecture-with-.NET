using Azure.Core;
using Azure.Identity;

namespace Presentation.BSA.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddPresentationLayerConfiguration(this IConfigurationBuilder configurationBuilder, bool useAzureKeyVault = true)
    {
        const string envVariable = "ASPNETCORE_ENVIRONMENT";
        string environment = Environment.GetEnvironmentVariable(envVariable) ?? throw new ArgumentNullException(envVariable);

        configurationBuilder.AddJsonFile("appsettings.json", optional: false);
        configurationBuilder.AddJsonFile($"appsettings.{environment}.json", optional: false);

        // Adding environment variables here allows overriding configuration from appsettings.json before Key Vault is added
        configurationBuilder.AddEnvironmentVariables();

        if (useAzureKeyVault)
        {
            AddAzureKeyVault(configurationBuilder, environment);
        }

        // Ensure environment variables are added last to override any other configuration
        configurationBuilder.AddEnvironmentVariables();

        return configurationBuilder;
    }

    private static void AddAzureKeyVault(IConfigurationBuilder configurationBuilder, string environment)
    {
        // Build a temporary configuration to access the Key Vault URL
        IConfiguration configuration = configurationBuilder.Build();

        string keyVaultUrl = configuration["KeyVault:Url"]
                             ?? throw new InvalidOperationException("Key Vault URL not found in configuration.");

        if (!Uri.TryCreate(keyVaultUrl, UriKind.Absolute, out Uri? validatedUri))
        {
            throw new InvalidOperationException($"Invalid Key Vault URL: '{keyVaultUrl}'. Ensure it is a properly formatted absolute URI.");
        }

        // Create a token credential to authenticate requests to the Key Vault
        TokenCredential tokenCredential = environment switch
        {
            "Development" or "test" => new VisualStudioCredential(), // Uses VS credentials for local development and testing
            _ => new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned) // Recommended for Azure-hosted environments
        };

        // Add Azure Key Vault as a configuration provider
        configurationBuilder.AddAzureKeyVault(validatedUri, tokenCredential);
    }
}