using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Operations.UseCases.ProcessPayment
{
    public class ProcessPaymentCommand : IRequest<Order>
    {
        public Guid UserId { get; set; }
        public List<ShoppingCartItem> Items { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpirationMonthYear { get; set; }
        public string CVV { get; set; }
        public string PostalCode { get; set; }
    }
}