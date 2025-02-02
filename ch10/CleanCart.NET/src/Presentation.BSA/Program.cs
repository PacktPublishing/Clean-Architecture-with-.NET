using Common.OpenTelemetry;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;
using System.Reflection;

namespace Presentation.BSA;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog(
                (context, config) =>
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
                            outputTemplate: "[{Timestamp:yyyy:MM:dd hh:mm:ss.fff tt}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
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
                            options.MaskProperties.Add("fullname");
                        }
                    });

                    config.ReadFrom.Configuration(context.Configuration);
                }
            )
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}