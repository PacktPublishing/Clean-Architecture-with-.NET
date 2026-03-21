using Application.Operations.Commands.User;
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
        FullName = $"{FirstName} {LastName}";
        DisplayName = FullName;

        // Log the user's information
        Logger.LogInformation("User authenticated: {UserId}, Email: {Email}, Name: {FullName}", _user.GetUserId(), Email, FullName);
    }

    private async Task EnsureUserCreated()
    {
        if (!string.IsNullOrWhiteSpace(Email))
        {
            await Mediator.Send(new EnsureUserExistsCommand
            {
                Email = Email,
                Username = Email,
                FullName = FullName
            });
        }
    }
}