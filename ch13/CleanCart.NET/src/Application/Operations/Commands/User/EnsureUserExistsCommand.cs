using MediatR;

namespace Application.Operations.Commands.User;

public class EnsureUserExistsCommand : IRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}