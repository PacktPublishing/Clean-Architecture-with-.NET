using Infrastructure.Startup;
using Microsoft.Extensions.Configuration;
using StartupOrchestration.NET;

namespace Presentation.Console;

internal sealed class PresentationServiceComposition : StartupOrchestrator<AppStartupOrchestrator>
{
    public PresentationServiceComposition(IConfigurationBuilder builder)
    {
        // Assign the externally provided IConfigurationBuilder so it can be used during configuration construction.
        // ⚠️ This property is virtual in the base class, so this class is marked as 'sealed' to avoid triggering
        // CA2214 (DoNotCallOverridableMethodsInConstructors). This prevents unintended behavior from derived classes
        // accessing overridden members before their constructors have run.
        DefaultConfigurationBuilder = builder;
    }

    protected override void AddConfigurationProviders(IConfigurationBuilder builder)
    {
        // To be removed.
    }
}