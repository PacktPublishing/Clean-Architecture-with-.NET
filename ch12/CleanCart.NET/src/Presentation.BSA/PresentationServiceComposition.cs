using Application.Interfaces.Auth;
using AutoMapper;
using Infrastructure.Composition;
using Presentation.BSA.Auth;
using Presentation.BSA.Extensions;
using Presentation.BSA.Mapping;
using Presentation.BSA.Services;
using ServiceComposition.NET;

namespace Presentation.BSA;

public class PresentationServiceComposition : ServiceCompositionRoot<AppServiceRegistrationPipeline>
{
    public PresentationServiceComposition()
    {
        // Add Presentation Services
        AddRegistration((services, config) => services.AddAuthentication(config));
        AddRegistration((services, config) => services.AddAuthorization(config));
        AddRegistration((services, config) => services.AddRazorPagesAndIdentityUI(config));
        AddRegistration((services, config) => services.AddMudBlazorServices(config));

        AddRegistration(services => services.AddCascadingAuthenticationState());
        AddRegistration(services => services.AddServerSideBlazor(null));

        // Add Services
        AddRegistration(services => services.AddScoped<ShoppingCartStateContainer>());
        AddRegistration(services => services.AddScoped<IAuthenticationService, BlazorAuthenticationService>());

        // Add AutoMapper Profiles
        AddRegistration(services => services.AddSingleton<Profile, PresentationMappingProfile>());
    }
}