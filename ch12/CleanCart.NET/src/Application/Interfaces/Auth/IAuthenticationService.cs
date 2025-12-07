using Domain.Entities;

namespace Application.Interfaces.Auth;

public interface IAuthenticationService
{
    Task<User?> GetCurrentUserAsync();
}