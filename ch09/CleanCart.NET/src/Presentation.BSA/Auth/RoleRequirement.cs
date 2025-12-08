using Microsoft.AspNetCore.Authorization;

namespace Presentation.BSA.Auth;

public class RoleRequirement(string roleName) : IAuthorizationRequirement
{
    public string RoleName { get; set; } = roleName;
}