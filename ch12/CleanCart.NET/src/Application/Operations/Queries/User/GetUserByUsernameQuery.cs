using MediatR;

namespace Application.Operations.Queries.User;

public class GetUserByUsernameQuery(string username) : IRequest<Domain.Entities.User?>
{
    public string Username { get; } = username;
}