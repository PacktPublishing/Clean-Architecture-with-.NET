using Application.Interfaces.Auth;
using AutoMapper;
using Domain.Enums;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor;
using MudBlazor.Services;
using Presentation.BSA.Auth;
using Presentation.BSA.Mapping;
using Presentation.BSA.Services;

namespace Presentation.BSA;

public class ServiceCompositionRoot(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add infrastructure services
        services.AddCoreLayerServices(configuration);

        // Add Microsoft Identity
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"));

        // Add custom authentication service
        services.AddScoped<IAuthenticationService, BlazorAuthenticationService>();

        AddAuthorization(services);

        services.AddRazorPages(options => {
                options.Conventions.AllowAnonymousToPage("/");
            })
            .AddMvcOptions(options => {
                AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();

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

        // Add AutoMapper profiles
        services.AddSingleton<Profile, PresentationMappingProfile>();

        // Add Presentation Services
        services.AddScoped<ShoppingCartState>();
    }

    static void AddAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // By default, all incoming requests will be authorized according to 
            // the default policy
            options.FallbackPolicy = options.DefaultPolicy;
        });

        services.AddScoped<IAuthorizationHandler, RoleHandler>();

        var roleNames = typeof(UserRole).GetFields().Select(x => x.Name);
        foreach (var roleName in roleNames)
        {
            services.AddAuthorizationCore(
                options =>
                    options.AddPolicy(
                        roleName,
                        policyBuilder =>
                        {
                            policyBuilder.AddRequirements(new RoleRequirement(roleName));
                        }
                    )
            );
        }
    }
}