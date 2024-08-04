using Application.Interfaces.Data;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Presentation.BSA.Extensions;
using System.Security.Claims;

namespace Presentation.BSA.Shared;

public partial class LoginDisplay
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;

    [Inject]
    private IUserRepository UserRepository { get; set; } = null!;

    private ClaimsPrincipal? _user;
    private string DisplayName { get; set; } = string.Empty;
    private string FullName { get; set; } = string.Empty;
    private string Email { get; set; } = string.Empty;
    private string FirstName { get; set; } = string.Empty;
    private string LastName { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;

        if (_user?.Identity?.IsAuthenticated == true)
        {
            InitializeUserProperties();
            await EnsureUserCreated();
        }
        else
        {
            DisplayName = "Not authenticated";
        }
    }

    private void InitializeUserProperties()
    {
        Email = _user.GetEmail();
        FirstName = _user.GetFirstName();
        LastName = _user.GetLastName();
        FullName = $"{FirstName} {LastName}";
        DisplayName = FullName;
    }

    private async Task EnsureUserCreated()
    {
        if (!string.IsNullOrEmpty(Email))
        {
            var user = await UserRepository.GetByUsernameAsync(Email);
            if (user == null)
            {
                // For testing purposes, grant all roles.
                // In a real-world scenario, you would assign roles based on the user's permissions and business logic.
                var defaultRoles = new List<UserRole> { UserRole.Administrator, UserRole.CustomerService };
                var newUser = new User(Email, Email, FullName, defaultRoles);
                await UserRepository.CreateUserAsync(newUser);
            }
        }
    }
}