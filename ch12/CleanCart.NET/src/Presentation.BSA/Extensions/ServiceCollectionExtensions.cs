using Domain.Enums;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor;
using MudBlazor.Services;
using Presentation.BSA.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Presentation.BSA.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"));
    }

    public static void AddRazorPagesAndIdentityUI(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRazorPages()
            .AddMicrosoftIdentityUI();
    }

    public static void AddMudBlazorServices(this IServiceCollection services, IConfiguration configuration)
    {
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

    public static void AddAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization(options =>
        {
            var roleNames = typeof(UserRole).GetFields().Select(x => x.Name);

            foreach (var roleName in roleNames)
            {
                options.AddPolicy(roleName, policyBuilder =>
                {
                    policyBuilder.AddRequirements(new RoleRequirement(roleName));
                });
            }
        });

        services.AddScoped<IAuthorizationHandler, RoleHandler>();
    }
}