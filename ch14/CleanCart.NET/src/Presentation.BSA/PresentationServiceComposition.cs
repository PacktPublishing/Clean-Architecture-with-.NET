using Application.Interfaces.Auth;
using AutoMapper;
using Infrastructure.Startup;
using Presentation.BSA.Auth;
using Presentation.BSA.Extensions;
using Presentation.BSA.Mapping;
using Presentation.BSA.Services;
using Presentation.Common.Extensions;
using StartupOrchestration.NET;

namespace Presentation.BSA;

public sealed class PresentationServiceComposition : StartupOrchestrator<AppStartupOrchestrator>
{
    public PresentationServiceComposition(IConfigurationBuilder builder)
    {
        // Assign the externally provided IConfigurationBuilder so it can be used during configuration construction.
        // ⚠️ This property is virtual in the base class, so this class is marked as 'sealed' to avoid triggering
        // CA2214 (DoNotCallOverridableMethodsInConstructors). This prevents unintended behavior from derived classes
        // accessing overridden members before their constructors have run.
        DefaultConfigurationBuilder = builder;

        // Add Presentation Services
        ServiceRegistrationExpressions.Add((services, config) => services.AddAuthentication(config));
        ServiceRegistrationExpressions.Add((services, config) => services.AddAuthorization(config));
        ServiceRegistrationExpressions.Add((services, config) => services.AddRazorPagesWithAuthorization(config));
        ServiceRegistrationExpressions.Add((services, config) => services.AddCascadingAuthenticationState());
        ServiceRegistrationExpressions.Add((services, config) => services.AddServerSideBlazor(null));
        ServiceRegistrationExpressions.Add((services, config) => services.AddMudBlazorServices(config));
        ServiceRegistrationExpressions.Add((services, config) => services.AddOpenTelemetry(config));

        // Add Services
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<ShoppingCartState>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IAuthenticationService, BlazorAuthenticationService>());

        // Add AutoMapper Profiles
        ServiceRegistrationExpressions.Add((services, config) => services.AddSingleton<Profile, PresentationMappingProfile>());
    }

    protected override void AddConfigurationProviders(IConfigurationBuilder builder)
    {
        // To be removed.
    }
}