using Common.OpenTelemetry;
using Infrastructure.Extensions;
using Presentation.BSA;
using Presentation.BSA.Extensions;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Configuration
// ------------------------------------------------------------

// Ensure configuration is resolved relative to the entry assembly
string assemblyLocation = Assembly.GetExecutingAssembly().Location;
string basePath = Directory.GetParent(assemblyLocation)!.FullName;

builder.Configuration
    .SetBasePath(basePath)
    .AddCoreLayerConfiguration()
    .AddPresentationLayerConfiguration();

// ------------------------------------------------------------
// Logging with Serilog
// ------------------------------------------------------------

builder.Host.UseSerilog((context, config) =>
{
    // Setup Serilog: https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/
    // Configuration: https://github.com/serilog/serilog-settings-configuration
    // Sinks        : https://github.com/serilog/serilog/wiki/Provided-Sinks
    // Wiki         : https://github.com/serilog/serilog/wiki
    // Docker       : docker run --rm -it -d -p 18888:18888 -p 4317:18889 -p 4318:18890 --name aspire-dashboard mcr.microsoft.com/dotnet/aspire-dashboard:9.0

    if (context.HostingEnvironment.IsDevelopment())
    {
        config.WriteTo.OpenTelemetry(options =>
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            options.Endpoint = "http://localhost:4318";
            options.Protocol = OtlpProtocol.HttpProtobuf;
            options.RestrictedToMinimumLevel = LogEventLevel.Debug;

            // Service metadata
            options.ResourceAttributes.Add("service.name", fvi.ProductName ?? "Unknown");
            options.ResourceAttributes.Add("service.version", fvi.FileVersion ?? "Unknown");
            options.ResourceAttributes.Add("service.instance.id", ServiceInstance.GetInstanceId());
            options.ResourceAttributes.Add("environment", context.HostingEnvironment.EnvironmentName);
        });

        config.WriteTo.Console(
            outputTemplate:
            "[{Timestamp:yyyy:MM:dd hh:mm:ss.fff tt}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
            theme: AnsiConsoleTheme.Literate
        );
    }

    config.Enrich.FromLogContext();
    config.Enrich.WithMachineName();
    config.Enrich.WithEnvironmentName();
    config.Enrich.WithThreadId();

    config.Enrich.WithSensitiveDataMasking(options =>
    {
        if (context.HostingEnvironment.IsDevelopment())
        {
            // Exclude automatically obfuscated properties
            options.ExcludeProperties.Add("email");
        }
        else
        {
            // Include specific properties to mask
            options.MaskProperties.Add(new MaskProperty { Name = "fullname" });
        }
    });

    config.ReadFrom.Configuration(context.Configuration);
}
);

// ------------------------------------------------------------
// Services
// ------------------------------------------------------------

// Delegate all service registration
var serviceComposition = new PresentationServiceComposition(builder.Configuration);
serviceComposition.ConfigureServices(builder.Services);

var app = builder.Build();

// ------------------------------------------------------------
// Middleware Pipeline
// ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static files are handled before endpoint execution
app.UseStaticFiles();

// Security middleware
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// ------------------------------------------------------------
// Endpoints
// ------------------------------------------------------------

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapRazorPages();
// Controllers are required for Microsoft.Identity.Web.UI endpoints such as:
// - /MicrosoftIdentity/Account/SignIn
// - /MicrosoftIdentity/Account/SignedOut
app.MapControllers();

await app.RunAsync();
