using Application.Operations.UseCases.ReviewOrderHistory;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Presentation.Functions;

/// <summary>
/// A timer-triggered Azure Function that runs every 20 seconds and logs recently placed orders.
/// Demonstrates how core business logic (via MediatR) can be reused in a scalable function context.
/// </summary>
public class RecentOrderLoggerFunction(IMediator mediator, ILogger<RecentOrderLoggerFunction> logger)
{
    [Function("RecentOrderLogger")]
    public async Task Run([TimerTrigger("*/20 * * * * *")] TimerInfo timer)
    {
        logger.LogInformation("Azure Function triggered at: {timestamp}", DateTime.UtcNow);

        var query = new GetRecentOrdersQuery(TimeSpan.FromSeconds(30));
        var recentOrders = (await mediator.Send(query)).ToList();

        if (!recentOrders.Any())
        {
            logger.LogInformation("No recent orders found.");
            return;
        }

        foreach (var order in recentOrders)
        {
            logger.LogInformation("Order ID: {OrderId} | Total: {Total} | Created: {Created}",
                order.Id, order.TotalAmount, order.CreatedOn);
        }

        if (timer.ScheduleStatus is not null)
        {
            logger.LogInformation("Next function run scheduled at: {next}", timer.ScheduleStatus.Next);
        }
    }
}