using Application.Interfaces.Auth;
using Infrastructure.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Common.Extensions;
using Presentation.Functions.Auth;
using StartupOrchestration.NET;

namespace Presentation.Functions;

/// <summary>
/// Startup class for the Azure Functions application, orchestrating the configuration and service registrations.
/// </summary>
public sealed class PresentationServiceComposition : StartupOrchestrator<AppStartupOrchestrator>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PresentationServiceComposition"/> class with the provided configuration builder.
    /// </summary>
    public PresentationServiceComposition(IConfigurationBuilder builder)
    {
        // Assign the externally provided IConfigurationBuilder so it can be used during configuration construction.
        // ⚠️ This property is virtual in the base class, so this class is marked as 'sealed' to avoid triggering
        // CA2214 (DoNotCallOverridableMethodsInConstructors). This prevents unintended behavior from derived classes
        // accessing overridden members before their constructors have run.
        DefaultConfigurationBuilder = builder;

        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IAuthenticationService, NoOpAuthenticationService>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddOpenTelemetry(config));
    }

    /// <inheritdoc />
    protected override void AddConfigurationProviders(IConfigurationBuilder builder)
    {
        // To be removed.
    }
}
