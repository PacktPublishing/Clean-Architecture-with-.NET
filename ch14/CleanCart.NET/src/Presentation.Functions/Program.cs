using Microsoft.Extensions.Hosting;
using Presentation.Common.Extensions;
using Presentation.Functions;

var builder = Host.CreateDefaultBuilder(args);
Startup? startup = null;

builder.ConfigureAppConfiguration(
    (context, configBuilder) =>
    {
        // Explicitly rely on the configuration builder provided by the host
        startup = new Startup(configBuilder);
    }
);

builder
    .ConfigureFunctionsWebApplication()
    .UseSerilogFromConfiguration();

builder.ConfigureServices(
    (context, services) =>
    {
        startup!.ConfigureServices(services);
    }
);

builder.Build().Run();
