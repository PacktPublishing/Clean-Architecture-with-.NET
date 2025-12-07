using Application.Interfaces.Data;
using MediatR;

namespace Application.Operations.Queries.User;

public class GetUserByEmailQueryHandler(IUserQueryRepository userQueryRepository) : IRequestHandler<GetUserByEmailQuery, Domain.Entities.User?>
{
    public Task<Domain.Entities.User?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        return userQueryRepository.GetByEmailAsync(request.Email);
    }
}