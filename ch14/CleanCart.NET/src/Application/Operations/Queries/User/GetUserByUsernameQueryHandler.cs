using Application.Interfaces.Data;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Operations.Queries.User
{
    public class GetUserByUsernameQueryHandler(IUserQueryRepository userQueryRepository) : IRequestHandler<GetUserByUsernameQuery, Domain.Entities.User?>
    {
        public Task<Domain.Entities.User?> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
        {
            return userQueryRepository.GetByUsernameAsync(request.Username);
        }
    }
}
