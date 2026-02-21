using Infrastructure.Helpers;
using Microsoft.Extensions.Hosting;
using Presentation.Common.Extensions;
using Presentation.Functions;
using System.Reflection;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateDefaultBuilder(args);

// ------------------------------------------------------------
// Configuration
// ------------------------------------------------------------

builder.ConfigureAppConfiguration(
    (context, configBuilder) =>
    {
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
        var serviceComposition = new PresentationServiceComposition();
        serviceComposition!.ConfigureServices(services, context.Configuration);
    }
);

// ------------------------------------------------------------
// Run
// ------------------------------------------------------------

await builder.Build().RunAsync();
