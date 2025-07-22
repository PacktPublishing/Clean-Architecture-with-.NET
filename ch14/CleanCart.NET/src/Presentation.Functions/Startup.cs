using Application.Interfaces.Auth;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Infrastructure.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Common.Extensions;
using Presentation.Functions.Auth;
using StartupOrchestration.NET;
using System.Reflection;

namespace Presentation.Functions;

/// <summary>
/// Startup class for the Azure Functions application, orchestrating the configuration and service registrations.
/// </summary>
public sealed class Startup : StartupOrchestrator<AppStartupOrchestrator>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class with the provided configuration builder.
    /// </summary>
    public Startup(IConfigurationBuilder builder)
    {
        // Assign the externally provided IConfigurationBuilder so it can be used during configuration construction.
        // ⚠️ This property is virtual in the base class, so this class is marked as 'sealed' to avoid triggering
        // CA2214 (DoNotCallOverridableMethodsInConstructors). This prevents unintended behavior from derived classes
        // accessing overridden members before their constructors have run.
        DefaultConfigurationBuilder = builder;

        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IAuthenticationService, NoOpAuthenticationService>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddOpenTelemetry(config));
    }

    protected override void SetBasePath(IConfigurationBuilder builder)
    {
        // Fix for isolated Azure Functions not finding the appsettings.json file in production
        // https://stackoverflow.com/questions/78119200/appsettings-for-azurefunction-on-net-8-isolated
        if (!AspNetEnvironmentHelper.IsDevelopment())
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath) ?? Directory.GetCurrentDirectory();
            builder.SetBasePath(assemblyDir);
        }
    }

    /// <inheritdoc />
    protected override void AddConfigurationProviders(IConfigurationBuilder builder)
    {
        builder.AddCoreLayerConfiguration();
        builder.AddPresentationLayerConfiguration();
    }
}
