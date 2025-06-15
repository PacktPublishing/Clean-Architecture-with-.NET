using FluentValidation;

namespace Application.Operations.UseCases.RemoveItemFromCart;

public class RemoveItemFromCartCommandValidator : AbstractValidator<RemoveItemFromCartCommand>
{
    public RemoveItemFromCartCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
