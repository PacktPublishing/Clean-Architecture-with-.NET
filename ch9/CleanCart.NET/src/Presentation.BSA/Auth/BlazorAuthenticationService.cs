using Application.Interfaces.Auth;
using Application.Interfaces.Data;
using Domain.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Presentation.BSA.Extensions;

namespace Presentation.BSA.Auth;

public class BlazorAuthenticationService(AuthenticationStateProvider authStateProvider, IUserRepository userRepository) : IAuthenticationService
{
    public async Task<User?> GetCurrentUserAsync()
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        if (state.User.Identity is { IsAuthenticated: false })
        {
            return null;
        }

        var authenticated = state.User.Identity?.IsAuthenticated ?? false;
        if (authenticated)
        {
            var email = state.User.GetEmail();
            if (!string.IsNullOrEmpty(email))
            {
                return await userRepository.GetByEmailAsync(email);
            }
        }

        return null;
    }
}