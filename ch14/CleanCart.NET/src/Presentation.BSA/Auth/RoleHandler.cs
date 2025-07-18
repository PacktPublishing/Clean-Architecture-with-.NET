using Application.Operations.Queries.User;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Presentation.BSA.Extensions;

namespace Presentation.BSA.Auth;

public class RoleHandler(IMediator mediator) : AuthorizationHandler<RoleRequirement>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        var username = context.User.GetEmail();
        if (string.IsNullOrEmpty(username))
        {
            return;
        }

        var query = new GetUserByUsernameQuery(username);
        var user = await mediator.Send(query);
        if (user == null)
        {
            return;
        }

        if (!Enum.TryParse(requirement.RoleName, out UserRole role))
        {
            return;
        }

        if (!user.Roles.Contains(role) && !user.Roles.Contains(UserRole.Administrator))
        {
            return;
        }

        // User has the expected role or is an administrator
        context.Succeed(requirement);
    }
}