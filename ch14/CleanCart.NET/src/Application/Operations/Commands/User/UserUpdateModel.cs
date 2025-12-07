using Domain.Enums;
using EntityAxis.MediatR.Commands;

namespace Application.Operations.Commands.User;

public class UserUpdateModel : IUpdateCommandModel<Domain.Entities.User, Guid>
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<UserRole> Roles { get; set; } = new();
}