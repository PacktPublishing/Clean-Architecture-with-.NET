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
using ServiceComposition.NET;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Infrastructure.Composition;

public class AppServiceRegistrationPipeline : ServiceRegistrationPipeline
{
    public AppServiceRegistrationPipeline()
    {
        // Add Options Support
        AddRegistration(services => services.AddOptions());
        AddRegistration(services => services.AddTransient(typeof(OptionsFactory<>)));
        AddRegistration(services => services.AddTransient(typeof(OptionsMonitor<>)));
        // Add Options
        AddRegistration((services, config) => services.AddOptions<SqlServerOptions>(config));
        AddRegistration((services, config) => services.AddOptions<PaymentGatewayApiOptions>(config));

        // Add SQL Server and Repositories
        AddRegistration(services => services.AddSqlServer());
        AddRegistration(services => services.AddScoped<IProductRepository, ProductRepository>());
        AddRegistration(services => services.AddScoped<IOrderRepository, OrderRepository>());
        AddRegistration(services => services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>());
        AddRegistration(services => services.AddScoped<IUserRepository, UserRepository>());

        // Add Services
        AddRegistration(services => services.AddScoped<IPaymentGateway, PaymentGateway>());

        // Add AutoMapper and Profiles
        AddRegistration(services => services.AddAutoMapper());
        // Profiles are registered in the DI Container to allow the Presentation Layer to
        // register its own profiles without needing to know about the Infrastructure Layer's profiles.
        AddRegistration(services => services.AddSingleton<Profile, ApplicationMappingProfile>());
        AddRegistration(services => services.AddSingleton<Profile, InfrastructureMappingProfile>());

        // Add HttpClients
        AddRegistration(services => services.AddPaymentGatewayApi());

        // Add UseCases
        AddRegistration(services => services.AddUseCases(typeof(IAddItemToCartUseCase)));
    }

    private static readonly ILogger SerilogLogger =
        new SerilogLoggerFactory(new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.ApplicationInsights(new TraceTelemetryConverter(), LogEventLevel.Information)
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy:MM:dd hh:mm:ss.fff tt}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
            .CreateLogger()
        ).CreateLogger(nameof(AppServiceRegistrationPipeline));

    /// <inheritdoc/>
    protected override ILogger Logger => SerilogLogger;
}