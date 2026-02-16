using Application.Interfaces.Data;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Presentation.BSA.Extensions;

namespace Presentation.BSA.Auth;

public class RoleHandler(IUserRepository userRepository) : AuthorizationHandler<RoleRequirement>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        var username = context.User.GetEmail();
        if (string.IsNullOrEmpty(username))
            return;

        var user = await userRepository.GetByUsernameAsync(username);
        if (user == null)
            return;

        if (!Enum.TryParse(requirement.RoleName, out UserRole role))
            return;

        if (!user.Roles.Contains(role) && !user.Roles.Contains(UserRole.Administrator))
            return;

        // User has the expected role or is an administrator
        context.Succeed(requirement);
    }
}