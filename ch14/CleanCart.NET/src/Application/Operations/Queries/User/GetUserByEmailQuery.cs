using MediatR;

namespace Application.Operations.Queries.User;

public class GetUserByEmailQuery(string email) : IRequest<Domain.Entities.User?>
{
    public string Email { get; } = email;
}