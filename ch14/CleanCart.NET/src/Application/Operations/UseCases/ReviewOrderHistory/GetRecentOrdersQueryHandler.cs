using Application.Interfaces.Data;
using Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Operations.UseCases.ReviewOrderHistory;

public class GetRecentOrdersQueryHandler : IRequestHandler<GetRecentOrdersQuery, IEnumerable<Order>>
{
    private readonly IOrderQueryRepository _orderQueryRepository;

    public GetRecentOrdersQueryHandler(IOrderQueryRepository orderQueryRepository)
    {
        _orderQueryRepository = orderQueryRepository;
    }

    public async Task<IEnumerable<Order>> Handle(GetRecentOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _orderQueryRepository.GetRecentOrdersAsync(request.WithinLast, cancellationToken);
    }
}