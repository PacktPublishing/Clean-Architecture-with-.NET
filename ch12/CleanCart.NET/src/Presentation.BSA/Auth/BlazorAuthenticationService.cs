using Application.Interfaces.Auth;
using Application.Operations.Queries.User;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;
using Presentation.BSA.Extensions;

namespace Presentation.BSA.Auth;

public class BlazorAuthenticationService(AuthenticationStateProvider authStateProvider, IMediator mediator) : IAuthenticationService
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
                var query = new GetUserByEmailQuery(email);
                return await mediator.Send(query);
            }
        }

        return null;
    }
}