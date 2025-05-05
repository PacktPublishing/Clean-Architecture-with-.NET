using MediatR;

namespace Application.Operations.Queries.User;

public class GetUserByUsernameQuery : IRequest<Domain.Entities.User?>
{
    public GetUserByUsernameQuery(string username)
    {
        Username = username;
    }

    public string Username { get; }
}