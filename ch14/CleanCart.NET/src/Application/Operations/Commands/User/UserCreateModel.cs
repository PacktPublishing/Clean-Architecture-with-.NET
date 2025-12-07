using Domain.Enums;

namespace Application.Operations.Commands.User;

public class UserCreateModel
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<UserRole> Roles { get; set; } = new();
}