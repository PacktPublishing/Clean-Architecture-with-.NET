using Infrastructure.Helpers;
using Microsoft.Extensions.Hosting;
using Presentation.Common.Extensions;
using Presentation.Functions;
using System.Reflection;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateDefaultBuilder(args);
PresentationServiceComposition? serviceComposition = null;

// ------------------------------------------------------------
// Configuration
// ------------------------------------------------------------

builder.ConfigureAppConfiguration(
    (context, configBuilder) =>
    {
        // Explicitly rely on the configuration builder provided by the host
        serviceComposition = new PresentationServiceComposition(configBuilder);

        // Fix for isolated Azure Functions not finding the appsettings.json file in deployed environment
        // https://stackoverflow.com/questions/78119200/appsettings-for-azurefunction-on-net-8-isolated
        if (!AspNetEnvironmentHelper.IsDevelopment())
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath) ?? Directory.GetCurrentDirectory();
            configBuilder.SetBasePath(assemblyDir);
        }

        configBuilder.AddCoreLayerConfiguration();
        configBuilder.AddPresentationLayerConfiguration();
    }
);

// ------------------------------------------------------------
// Services + Functions
// ------------------------------------------------------------

builder
    .ConfigureFunctionsWebApplication()
    .UseSerilogFromConfiguration();

builder.ConfigureServices(
    (context, services) =>
    {
        serviceComposition!.ConfigureServices(services);
    }
);

// ------------------------------------------------------------
// Run
// ------------------------------------------------------------

await builder.Build().RunAsync();
