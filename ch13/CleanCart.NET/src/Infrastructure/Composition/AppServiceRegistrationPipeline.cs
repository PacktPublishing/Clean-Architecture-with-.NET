using Application.Interfaces.Services.Payment;
using Application.Mapping;
using Application.Operations.Commands.User;
using Application.Operations.UseCases.ProcessPayment;
using AutoMapper;
using Domain.Entities;
using EntityAxis.MediatR.Registration;
using FluentValidation;
using Infrastructure.Configuration;
using Infrastructure.Extensions;
using Infrastructure.Mapping;
using Infrastructure.MediatR;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using ServiceComposition.NET;
using EntityAxis.KeyMappers;
using EntityAxis.Registration;
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
        AddRegistration(services => services.AddSingleton<IKeyMapper<Guid, Guid>, IdentityKeyMapper<Guid>>());
        AddRegistration(services => services.AddEntityAxisCommandAndQueryServicesFromAssembly<OrderQueryRepository>(ServiceLifetime.Scoped));

        // Add Services
        AddRegistration(services => services.AddScoped<IPaymentGateway, PaymentGateway>());

        // Add AutoMapper and Profiles
        AddRegistration(services => services.AddAutoMapper());
        // Profiles are registered in the DI Container to allow the Presentation Layer to register its own profiles without needing to know about the Infrastructure Layer's profiles.
        AddRegistration(services => services.AddSingleton<Profile, ApplicationMappingProfile>());
        AddRegistration(services => services.AddSingleton<Profile, InfrastructureMappingProfile>());

        // Add HttpClients
        AddRegistration(services => services.AddPaymentGatewayApi());

        // Add MediatR
        AddRegistration(services => services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(ProcessPaymentCommand).Assembly)));
        AddRegistration(services => services.AddValidatorsFromAssemblyContaining<ProcessPaymentCommandValidator>(ServiceLifetime.Scoped, null, false));
        AddRegistration(services => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));

        // Add EntityAxis Handlers
        AddRegistration(services => services.AddEntityAxisCommandHandlers<User, Guid>(builder => builder.AddCreate<UserCreateModel>().AddUpdate<UserUpdateModel>()));
        AddRegistration(services => services.AddEntityAxisQueryHandlers<User, Guid>(builder => builder.AddAllQueries()));
        AddRegistration(services => services.AddEntityAxisQueryHandlers<Order, Guid>(builder => builder.AddAllQueries()));
        AddRegistration(services => services.AddEntityAxisQueryHandlers<Product, Guid>(builder => builder.AddAllQueries()));
        AddRegistration(services => services.AddEntityAxisQueryHandlers<ShoppingCart, Guid>(builder => builder.AddAllQueries()));
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