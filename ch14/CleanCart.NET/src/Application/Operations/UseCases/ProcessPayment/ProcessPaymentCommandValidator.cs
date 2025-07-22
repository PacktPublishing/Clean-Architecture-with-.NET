using Domain.Entities;
using FluentValidation;

namespace Application.Operations.UseCases.ProcessPayment;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new ShoppingCartItemValidator());

        RuleFor(x => x.CardNumber).CreditCard().WithMessage("Invalid card number.");
        RuleFor(x => x.CardHolderName).NotEmpty();
        RuleFor(x => x.ExpirationMonthYear).Matches(@"^(0[1-9]|1[0-2])\/\d{2}$").WithMessage("Expiration must be in MM/YY format.");
        RuleFor(x => x.CVV).Matches(@"^\d{3,4}$").WithMessage("CVV must be 3 or 4 digits.");
        RuleFor(x => x.PostalCode).NotEmpty();
    }
}

public class ShoppingCartItemValidator : AbstractValidator<ShoppingCartItem>
{
    public ShoppingCartItemValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ProductPrice).GreaterThanOrEqualTo(0);
    }
}
