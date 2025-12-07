using Common.OpenTelemetry;
using Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;

namespace Presentation.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        // Get assembly and file version info
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

        // Register ActivitySource and Meter for tracing and metrics
        services.AddSingleton(new ActivitySource(fvi.ProductName!, fvi.FileVersion));
        services.AddSingleton(new Meter(fvi.ProductName!, fvi.FileVersion));

        // Example: Use OpenTelemetry with OTLP and the standalone Aspire Dashboard
        // https://learn.microsoft.com/dotnet/core/diagnostics/observability-otlp-example
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Add ActivitySource for tracing
                    .AddSource(fvi.ProductName!);

                if (!AspNetEnvironmentHelper.IsDevelopment() && !AspNetEnvironmentHelper.IsTest())
                {
                    builder.AddAzureMonitorTraceExporter();
                }
                else
                {
                    builder.AddOtlpExporter();
                }
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(fvi.ProductName!)
                    // Metrics provides by ASP.NET Core in .NET
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel");

                if (!AspNetEnvironmentHelper.IsDevelopment() && !AspNetEnvironmentHelper.IsTest())
                {
                    builder.AddAzureMonitorMetricExporter();
                }
                else
                {
                    builder.AddOtlpExporter();
                }
            })
            // Custom resource attributes should be added AFTER AzureMonitor to override the default ResourceDetectors.
            .ConfigureResource(builder =>
            {
                // Learn About OpenTelemetry Semantic Conventions
                // https://opentelemetry.io/docs/specs/semconv/

                // Custom attributes are not collected by default in Azure Monitor and must be enabled
                // https://learn.microsoft.com/azure/azure-monitor/app/metrics-overview?tabs=standard#custom-metrics-dimensions-and-preaggregation

                string environment = AspNetEnvironmentHelper.GetRequiredEnvironmentName();
                builder.AddService(fvi.ProductName!, serviceNamespace: "Project Odyssey", serviceVersion: fvi.FileVersion, serviceInstanceId: ServiceInstance.GetInstanceId());
                builder.AddAttributes([new KeyValuePair<string, object>("deployment.environment.name", environment)]);

                // GitHub Issue: Azure Monitor Custom Attributes are not exported for tracing and metrics
                // https://github.com/Azure/azure-sdk-for-net/issues/46020
            });
    }
}