using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.UseCases;
using Application.Mapping;
using AutoMapper;
using Infrastructure.Configuration;
using Infrastructure.Extensions;
using Infrastructure.Mapping;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using StartupOrchestration.NET;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Infrastructure.Startup;

public class AppStartupOrchestrator : ServiceRegistrationOrchestrator
{
    public AppStartupOrchestrator()
    {
        // Add Options Support
        ServiceRegistrationExpressions.Add((services, config) => services.AddOptions());
        ServiceRegistrationExpressions.Add((services, config) => services.AddTransient(typeof(OptionsFactory<>)));
        ServiceRegistrationExpressions.Add((services, config) => services.AddTransient(typeof(OptionsMonitor<>)));
        // Add Options
        ServiceRegistrationExpressions.Add((services, config) => services.AddOptions<SqlServerOptions>(config));
        ServiceRegistrationExpressions.Add((services, config) => services.AddOptions<PaymentGatewayApiOptions>(config));

        // Add SQL Server and Repositories
        ServiceRegistrationExpressions.Add((services, config) => services.AddSqlServer());
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IProductRepository, ProductRepository>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IOrderRepository, OrderRepository>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IUserRepository, UserRepository>());

        // Add Services
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IPaymentGateway, PaymentGateway>());

        // Add AutoMapper and Profiles
        ServiceRegistrationExpressions.Add((services, config) => services.AddAutoMapper());
        // Profiles are registered in the DI Container to allow the Presentation Layer to register its own profiles without needing to know about the Infrastructure Layer's profiles.
        ServiceRegistrationExpressions.Add((services, config) => services.AddSingleton<Profile, ApplicationMappingProfile>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddSingleton<Profile, InfrastructureMappingProfile>());

        // Add HttpClients
        ServiceRegistrationExpressions.Add((services, config) => services.AddPaymentGatewayApi());

        // Add UseCases
        ServiceRegistrationExpressions.Add((services, config) => services.AddUseCases(typeof(IAddItemToCartUseCase)));
    }

    /// <inheritdoc/>
    protected override ILogger StartupLogger => new SerilogLoggerFactory(new LoggerConfiguration()
        .Enrich.FromLogContext()
        .MinimumLevel.Verbose()
        .WriteTo.ApplicationInsights(new TraceTelemetryConverter(), LogEventLevel.Information)
        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy:MM:dd hh:mm:ss.fff tt}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
        .CreateLogger()
    ).CreateLogger(nameof(AppStartupOrchestrator));
}