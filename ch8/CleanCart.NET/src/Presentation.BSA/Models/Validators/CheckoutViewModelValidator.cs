using FluentValidation;
using Presentation.BSA.Models.ViewModels;

namespace Presentation.BSA.Models.Validators;

public class CheckoutViewModelValidator : AbstractValidator<CheckoutViewModel>
{
    public CheckoutViewModelValidator()
    {
        RuleFor(x => x.CardNumber).NotEmpty().CreditCard();
        RuleFor(x => x.CardHolderName).NotEmpty().Length(1, 100);
        RuleFor(x => x.ExpirationMonthYear).NotEmpty().Length(5).Matches(@"^\d{2}/\d{2}$");
        RuleFor(x => x.CVV).NotEmpty().Length(3).Matches(@"^\d{3}$");
        RuleFor(x => x.PostalCode).NotEmpty().Length(5).Matches(@"^\d{5}$");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue =>
        async (model, propertyName) =>
        {
            var result = await ValidateAsync(
                ValidationContext<CheckoutViewModel>.CreateWithOptions((CheckoutViewModel)model, x => x.IncludeProperties(propertyName))
            );
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
}