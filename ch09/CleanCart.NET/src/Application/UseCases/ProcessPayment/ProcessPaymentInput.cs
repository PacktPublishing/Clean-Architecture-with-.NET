using Domain.Entities;

namespace Application.UseCases.ProcessPayment;

public class ProcessPaymentInput
{
    public Guid UserId { get; set; }
    public List<ShoppingCartItem> Items { get; set; } = new();
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpirationMonthYear { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}