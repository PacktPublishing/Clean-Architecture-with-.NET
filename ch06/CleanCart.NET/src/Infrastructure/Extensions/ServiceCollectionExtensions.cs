using Application.Interfaces.Services.Payment;
using Infrastructure.Clients;
using Infrastructure.Configuration;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.AddOptions()
            .AddTransient(typeof(OptionsFactory<>))
            .AddTransient(typeof(OptionsMonitor<>))
            .AddOptions<PaymentGatewayApiOptions>(configuration);

        // Services
        services.AddScoped<IPaymentGateway, PaymentGateway>();

        // HttpClients
        services.AddPaymentGatewayApi();

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
}