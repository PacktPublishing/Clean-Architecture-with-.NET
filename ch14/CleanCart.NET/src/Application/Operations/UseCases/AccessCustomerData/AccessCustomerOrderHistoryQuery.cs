using Domain.Entities;
using MediatR;

namespace Application.Operations.UseCases.AccessCustomerData;

public class AccessCustomerOrderHistoryQuery(Guid authorizationUserId, Guid customerUserId)
    : IRequest<IEnumerable<Order>>
{
    public Guid AuthorizationUserId { get; } = authorizationUserId;
    public Guid CustomerUserId { get; } = customerUserId;
}