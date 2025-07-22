using MediatR;

namespace Application.Operations.Queries.User;

public class GetUserByEmailQuery : IRequest<Domain.Entities.User?>
{
    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }

    public string Email { get; }
}