using Application.Interfaces.Data;
using Domain.Entities;
using MediatR;

namespace Application.Operations.UseCases.ReviewOrderHistory;

public class GetRecentOrdersQueryHandler(IOrderQueryRepository orderQueryRepository)
    : IRequestHandler<GetRecentOrdersQuery, IEnumerable<Order>>
{
    public async Task<IEnumerable<Order>> Handle(GetRecentOrdersQuery request, CancellationToken cancellationToken)
    {
        return await orderQueryRepository.GetRecentOrdersAsync(request.WithinLast, cancellationToken);
    }
}