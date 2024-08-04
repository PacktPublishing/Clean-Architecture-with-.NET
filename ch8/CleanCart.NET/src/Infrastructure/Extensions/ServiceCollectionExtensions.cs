using Application.Interfaces.Data;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.UseCases;
using Infrastructure.Clients;
using Infrastructure.Configuration;
using Infrastructure.Mapping;
using Infrastructure.Persistence.EntityFramework;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using System;
using System.Linq;
using System.Reflection;
using Application.Mapping;
using AutoMapper;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.AddOptions()
            .AddTransient(typeof(OptionsFactory<>))
            .AddTransient(typeof(OptionsMonitor<>))
            .AddOptions<SqlServerOptions>(configuration)
            .AddOptions<PaymentGatewayApiOptions>(configuration);

        // Data
        services.AddSqlServer()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IOrderRepository, OrderRepository>()
            .AddScoped<IShoppingCartRepository, ShoppingCartRepository>()
            .AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<IPaymentGateway, PaymentGateway>();

        // AutoMapper
        services.AddSingleton<Profile, ApplicationMappingProfile>();
        services.AddSingleton<Profile, InfrastructureMappingProfile>();
        services.AddSingleton(serviceProvider =>
        {
            var configurationExpression = new MapperConfigurationExpression();
            var profiles = serviceProvider.GetServices<Profile>();
            foreach (Profile profile in profiles)
            {
                configurationExpression.AddProfile(profile);
            }
            configurationExpression.ConstructServicesUsing(serviceProvider.GetService);
            var mapperConfiguration = new MapperConfiguration(configurationExpression);
            return mapperConfiguration.CreateMapper();
        });

        // HttpClients
        services.AddPaymentGatewayApi();

        // UseCases
        services.AddUseCases(typeof(IAddItemToCartUseCase));

        return services;
    }

    public static IServiceCollection AddOptions<T>(this IServiceCollection services, IConfiguration configuration) where T : class, new()
    {
        string sectionKey = typeof(T).Name;
        string modifiedSectionKey = sectionKey.Replace("Options", "");
        IConfigurationSection section = configuration.GetSection(modifiedSectionKey);

        if (!section.Exists())
        {
            // If the section doesn't exist, try the original section key.
            section = configuration.GetSection(sectionKey);
        }

        if (section.GetChildren().Any())
        {
            var options = new T();
            section.Bind(options);
            services.Configure<T>(section);
        }

        // Handle the case where the section doesn't exist or is empty.
        // You can log a warning or throw an exception as needed.
        // For example:
        // throw new InvalidOperationException($"Configuration section '{sectionKey}' does not exist or is empty.");
        return services;
    }

    private static IServiceCollection AddSqlServer(this IServiceCollection services)
    {
        services.AddDbContextFactory<CoreDbContext>(
            (serviceProvider, builder) =>
            {
                var options = serviceProvider.GetRequiredService<IOptionsMonitor<SqlServerOptions>>().CurrentValue;

                if (string.IsNullOrEmpty(options.ConnectionString))
                    throw new ArgumentNullException(nameof(options.ConnectionString));

                builder.UseSqlServer(options.ConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.MigrationsAssembly(typeof(CoreDbContext).Assembly.FullName);
                        sqlServerOptions.EnableRetryOnFailure();
                    }
                );

                if (options.EnableSensitiveDataLogging)
                    builder.EnableSensitiveDataLogging();
            }
        );

        return services;
    }

    private static IServiceCollection AddPaymentGatewayApi(this IServiceCollection services)
    {
        services.AddHttpClient<IPaymentGatewayApi>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptionsMonitor<PaymentGatewayApiOptions>>().CurrentValue;
                client.BaseAddress = new Uri(options.BaseUrl);
            }
        ).AddTypedClient(RestService.For<IPaymentGatewayApi>);
        return services;
    }

    public static IServiceCollection AddUseCases(this IServiceCollection services, Type knownInterface)
    {
        var assembly = Assembly.GetAssembly(knownInterface);
        if (assembly == null) throw new InvalidOperationException("Assembly could not be found.");

        var interfaceNamespace = knownInterface.Namespace;
        if (interfaceNamespace == null) throw new InvalidOperationException("Namespace could not be determined.");

        var interfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.Namespace == interfaceNamespace);

        foreach (var @interface in interfaces)
        {
            var implementation = assembly.GetTypes()
                .FirstOrDefault(t => t.IsClass && !t.IsAbstract && @interface.IsAssignableFrom(t));

            if (implementation != null)
            {
                services.AddScoped(@interface, implementation);
            }
        }

        return services;
    }
}