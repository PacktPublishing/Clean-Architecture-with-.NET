using Application.Operations.Commands.User;
using Application.Operations.Queries.User;
using Domain.Entities;
using Domain.Enums;
using EntityAxis.MediatR.Commands;
using MediatR;
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
    private IMediator Mediator { get; set; } = null!;

    [Inject]
    private ILogger<LoginDisplay> Logger { get; set; } = null!;

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
        string userId = _user.GetUserId();
        FullName = $"{FirstName} {LastName}";
        DisplayName = FullName;

        // Log the user's information
        Logger.LogInformation("User authenticated: {UserId}, Email: {Email}, Name: {FullName}", userId, Email, FullName);
    }

    private async Task EnsureUserCreated()
    {
        if (!string.IsNullOrEmpty(Email))
        {
            // TODO: Migrate this business logic to the Application layer
            var query = new GetUserByUsernameQuery(Email);
            var user = await Mediator.Send(query);
            if (user == null)
            {
                // For testing purposes, grant all roles.
                // In a real-world scenario, you would assign roles based on the user's permissions and business logic.
                var defaultRoles = new List<UserRole> { UserRole.Administrator, UserRole.CustomerService };
                var command = new CreateEntityCommand<UserCreateModel, User, Guid>(new UserCreateModel
                {
                    Username = Email,
                    Email = Email,
                    FullName = FullName,
                    Roles = defaultRoles
                });
                await Mediator.Send(command);
            }
        }
    }
}