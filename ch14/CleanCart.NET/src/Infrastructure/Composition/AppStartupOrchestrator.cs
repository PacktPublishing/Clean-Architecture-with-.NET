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
using ServiceComposition.NET;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Infrastructure.Composition;

public class AppServiceRegistrationPipeline : ServiceRegistrationPipeline
{
    public AppServiceRegistrationPipeline()
    {
        // Add Options Support
        AddRegistration((services, config) => services.AddOptions());
        AddRegistration((services, config) => services.AddTransient(typeof(OptionsFactory<>)));
        AddRegistration((services, config) => services.AddTransient(typeof(OptionsMonitor<>)));
        // Add Options
        AddRegistration((services, config) => services.AddOptions<SqlServerOptions>(config));
        AddRegistration((services, config) => services.AddOptions<PaymentGatewayApiOptions>(config));

        // Add SQL Server and Repositories
        AddRegistration((services, config) => services.AddSqlServer());
        AddRegistration((services, config) => services.AddSingleton<IKeyMapper<Guid, Guid>, IdentityKeyMapper<Guid>>());
        AddRegistration((services, config) => services.AddEntityAxisCommandAndQueryServicesFromAssembly<ProductQueryRepository>(ServiceLifetime.Scoped));

        // Add Services
        AddRegistration((services, config) => services.AddScoped<IPaymentGateway, PaymentGateway>());
        AddRegistration((services, config) => services.AddScoped<IMetricsService, MetricsService>());

        // Add AutoMapper and Profiles
        AddRegistration((services, config) => services.AddAutoMapper());
        // Profiles are registered in the DI Container to allow the Presentation Layer to register its own profiles without needing to know about the Infrastructure Layer's profiles.
        AddRegistration((services, config) => services.AddSingleton<Profile, ApplicationMappingProfile>());
        AddRegistration((services, config) => services.AddSingleton<Profile, InfrastructureMappingProfile>());

        // Add HttpClients
        AddRegistration((services, config) => services.AddPaymentGatewayApi());

        // Add MediatR
        AddRegistration((services, config) => services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(AddItemToCartCommandHandler).Assembly)));
        AddRegistration((services, config) => services.AddValidatorsFromAssemblyContaining<AddItemToCartCommandValidator>(ServiceLifetime.Scoped, null, false));
        AddRegistration((services, config) => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        AddRegistration((services, config) => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsAndLoggingPipelineBehavior<,>)));

        // Add EntityAxis Handlers
        AddRegistration((services, config) => services.AddEntityAxisCommandHandlers<User, Guid>(builder => builder.AddCreate<UserCreateModel>().AddUpdate<UserUpdateModel>()));
        AddRegistration((services, config) => services.AddEntityAxisQueryHandlers<User, Guid>(builder => builder.AddAllQueries()));
        AddRegistration((services, config) => services.AddEntityAxisQueryHandlers<Order, Guid>(builder => builder.AddAllQueries()));
        AddRegistration((services, config) => services.AddEntityAxisQueryHandlers<Product, Guid>(builder => builder.AddAllQueries()));
        AddRegistration((services, config) => services.AddEntityAxisQueryHandlers<ShoppingCart, Guid>(builder => builder.AddAllQueries()));

        // Add Health Checks
        AddRegistration((services, config) => services.AddAppHealthChecks());

        // Add System Clock
        AddRegistration((services, config) => services.AddSingleton<ISystemClock, SystemClock>());
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