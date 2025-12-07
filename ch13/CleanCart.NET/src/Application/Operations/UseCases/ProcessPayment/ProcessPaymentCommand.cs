using Domain.Entities;
using MediatR;

namespace Application.Operations.UseCases.ProcessPayment;

public class ProcessPaymentCommand : IRequest<Order>
{
    public Guid UserId { get; set; }
    public List<ShoppingCartItem> Items { get; set; } = new();
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpirationMonthYear { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}