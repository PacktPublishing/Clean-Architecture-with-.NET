using Domain.Enums;

namespace Domain.Entities;

public class User(string username, string email, string fullName, List<UserRole> roles)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Username { get; private set; } = username;
    public string Email { get; private set; } = email;
    public string FullName { get; private set; } = fullName;
    public List<UserRole> Roles { get; } = roles;

    public void AddRole(UserRole role)
    {
        Roles.Add(role);
    }

    public void RemoveRole(UserRole role)
    {
        Roles.Remove(role);
    }
}