using Domain.Entities;
using MediatR;

namespace Application.Operations.UseCases.ReviewOrderHistory;

public class GetRecentOrdersQuery(TimeSpan withinLast) : IRequest<IEnumerable<Order>>
{
    public TimeSpan WithinLast { get; set; } = withinLast;
}