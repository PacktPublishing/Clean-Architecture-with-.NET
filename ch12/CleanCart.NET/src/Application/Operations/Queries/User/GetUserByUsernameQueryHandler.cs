using Application.Interfaces.Data;
using MediatR;

namespace Application.Operations.Queries.User;

public class GetUserByUsernameQueryHandler(IUserQueryRepository userQueryRepository) : IRequestHandler<GetUserByUsernameQuery, Domain.Entities.User?>
{
    public Task<Domain.Entities.User?> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        return userQueryRepository.GetByUsernameAsync(request.Username);
    }
}