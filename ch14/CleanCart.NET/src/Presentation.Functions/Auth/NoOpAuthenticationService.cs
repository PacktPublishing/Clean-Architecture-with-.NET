using Application.Interfaces.Auth;
using Domain.Entities;

namespace Presentation.Functions.Auth;

internal class NoOpAuthenticationService : IAuthenticationService
{
    public Task<User?> GetCurrentUserAsync()
    {
        // No operation implementation, returns null
        return Task.FromResult<User?>(null);
    }
}