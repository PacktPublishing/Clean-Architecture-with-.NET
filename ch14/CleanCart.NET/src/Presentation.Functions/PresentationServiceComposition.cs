using Application.Interfaces.Auth;
using Infrastructure.Composition;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Common.Extensions;
using Presentation.Functions.Auth;
using ServiceComposition.NET;

namespace Presentation.Functions;

/// <summary>
/// Azure Functions service composition root that executes the application
/// registration pipeline and adds function-specific services.
/// </summary>
public class PresentationServiceComposition : ServiceCompositionRoot<AppServiceRegistrationPipeline>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PresentationServiceComposition"/> class with the provided configuration builder.
    /// </summary>
    public PresentationServiceComposition()
    {
        AddRegistration(services => services.AddScoped<IAuthenticationService, NoOpAuthenticationService>());
        AddRegistration((services, config) => services.AddOpenTelemetry(config));
    }
}
