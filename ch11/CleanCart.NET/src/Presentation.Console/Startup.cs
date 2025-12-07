using Infrastructure.Extensions;
using Infrastructure.Startup;
using Microsoft.Extensions.Configuration;
using StartupOrchestration.NET;

namespace Presentation.Console;

internal class Startup : StartupOrchestrator<AppStartupOrchestrator>
{
    protected override void AddConfigurationProviders(IConfigurationBuilder builder)
    {
        string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string path = Directory.GetParent(assemblyLocation)!.FullName;
        builder.SetBasePath(path);
        builder.AddCoreLayerConfiguration();
    }
}