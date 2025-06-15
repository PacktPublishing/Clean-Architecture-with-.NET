using FluentValidation;

namespace Application.Operations.UseCases.AccessCustomerData;

public class AccessCustomerShoppingCartQueryValidator : AbstractValidator<AccessCustomerShoppingCartQuery>
{
    public AccessCustomerShoppingCartQueryValidator()
    {
        RuleFor(x => x.CustomerUserId)
            .NotEmpty()
            .WithMessage("User ID cannot be empty.");
    }
}