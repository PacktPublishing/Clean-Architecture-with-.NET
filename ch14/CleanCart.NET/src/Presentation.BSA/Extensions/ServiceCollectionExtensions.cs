using Domain.Enums;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor;
using MudBlazor.Services;
using Presentation.BSA.Auth;

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

    public static void AddRazorPagesWithAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRazorPages(options =>
            {
                options.Conventions.AllowAnonymousToPage("/");
            })
            // Add Authorization to Razor Pages
            .AddMvcOptions(options =>
            {
                AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
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