using Domain.Enums;
using EntityAxis.Abstractions;

namespace Domain.Entities;

public class User : IEntityId<Guid>
{
    private readonly List<UserRole> _roles = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Username { get; }
    public string Email { get; }
    public string FullName { get; }
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    public User(string username, string email, string fullName, IEnumerable<UserRole> roles)
    {
        Username = username;
        Email = email;
        FullName = fullName;
        _roles.AddRange(roles);
    }

    public void AddRole(UserRole role)
    {
        if (!_roles.Contains(role))
        {
            _roles.Add(role);
        }
    }

    public void RemoveRole(UserRole role)
    {
        _roles.Remove(role);
    }
}
