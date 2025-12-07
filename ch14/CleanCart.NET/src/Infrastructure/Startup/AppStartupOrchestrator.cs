using Application.Interfaces.Services.Payment;
using Application.Interfaces.Services.Telemetry;
using Application.Mapping;
using Application.Operations.Commands.User;
using Application.Operations.UseCases.AddItemToCart;
using AutoMapper;
using Domain.Entities;
using EntityAxis.KeyMappers;
using EntityAxis.MediatR.Registration;
using EntityAxis.Registration;
using FluentValidation;
using Infrastructure.Configuration;
using Infrastructure.Extensions;
using Infrastructure.Mapping;
using Infrastructure.MediatR;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
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
        ServiceRegistrationExpressions.Add((services, config) => services.AddSingleton<IKeyMapper<Guid, Guid>, IdentityKeyMapper<Guid>>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddEntityAxisCommandAndQueryServicesFromAssembly<ProductQueryRepository>(ServiceLifetime.Scoped));

        // Add Services
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IPaymentGateway, PaymentGateway>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IMetricsService, MetricsService>());

        // Add AutoMapper and Profiles
        ServiceRegistrationExpressions.Add((services, config) => services.AddAutoMapper());
        // Profiles are registered in the DI Container to allow the Presentation Layer to register its own profiles without needing to know about the Infrastructure Layer's profiles.
        ServiceRegistrationExpressions.Add((services, config) => services.AddSingleton<Profile, ApplicationMappingProfile>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddSingleton<Profile, InfrastructureMappingProfile>());

        // Add HttpClients
        ServiceRegistrationExpressions.Add((services, config) => services.AddPaymentGatewayApi());

        // Add MediatR
        ServiceRegistrationExpressions.Add((services, config) => services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(AddItemToCartCommandHandler).Assembly)));
        ServiceRegistrationExpressions.Add((services, config) => services.AddValidatorsFromAssemblyContaining<AddItemToCartCommandValidator>(ServiceLifetime.Scoped, null, false));
        ServiceRegistrationExpressions.Add((services, config) => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        ServiceRegistrationExpressions.Add((services, config) => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsAndLoggingPipelineBehavior<,>)));

        // Add EntityAxis Handlers
        ServiceRegistrationExpressions.Add((services, config) => services.AddEntityAxisCommandHandlers<User, Guid>(builder => builder.AddCreate<UserCreateModel>().AddUpdate<UserUpdateModel>()));
        ServiceRegistrationExpressions.Add((services, config) => services.AddEntityAxisQueryHandlers<User, Guid>(builder => builder.AddAllQueries()));
        ServiceRegistrationExpressions.Add((services, config) => services.AddEntityAxisQueryHandlers<Order, Guid>(builder => builder.AddAllQueries()));
        ServiceRegistrationExpressions.Add((services, config) => services.AddEntityAxisQueryHandlers<Product, Guid>(builder => builder.AddAllQueries()));
        ServiceRegistrationExpressions.Add((services, config) => services.AddEntityAxisQueryHandlers<ShoppingCart, Guid>(builder => builder.AddAllQueries()));

        // Add Health Checks
        ServiceRegistrationExpressions.Add((services, config) => services.AddAppHealthChecks());

        // Add System Clock
        ServiceRegistrationExpressions.Add((services, config) => services.AddSingleton<ISystemClock, SystemClock>());
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