using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Operations.UseCases.ReviewOrderHistory;

public class GetRecentOrdersQuery : IRequest<IEnumerable<Order>>
{
    public GetRecentOrdersQuery(TimeSpan withinLast)
    {
        WithinLast = withinLast;
    }

    public TimeSpan WithinLast { get; set; }
}