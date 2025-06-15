using Domain.Enums;
using EntityAxis.Abstractions;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class User : IEntityId<Guid>
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string FullName { get; private set; }
        public List<UserRole> Roles { get; }

        public User(string username, string email, string fullName, List<UserRole> roles)
        {
            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            FullName = fullName;
            Roles = roles;
        }

        public void AddRole(UserRole role)
        {
            Roles.Add(role);
        }

        public void RemoveRole(UserRole role)
        {
            Roles.Remove(role);
        }
    }
}
