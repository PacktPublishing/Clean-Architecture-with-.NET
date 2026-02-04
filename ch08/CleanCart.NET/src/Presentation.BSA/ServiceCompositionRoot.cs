using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor;
using MudBlazor.Services;
using Presentation.BSA.Data;

namespace Presentation.BSA;

public class ServiceCompositionRoot(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add infrastructure services
        services.AddCoreLayerServices(configuration);

        // Add presentation services
        services.AddSingleton<WeatherForecastService>();

        // Add Microsoft Identity
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"));

        services
            .AddRazorPages()
            .AddMicrosoftIdentityUI();

        services.AddCascadingAuthenticationState();

        // Add presentation services to the container.
        services.AddServerSideBlazor();

        // Add MudBlazor
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            config.SnackbarConfiguration.PreventDuplicates = false;
            config.SnackbarConfiguration.NewestOnTop = true;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 4000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });
    }
}