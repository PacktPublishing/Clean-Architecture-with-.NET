using FluentValidation;

namespace Application.Operations.UseCases.CalculateCartTotal;

public class CalculateCartTotalQueryValidator : AbstractValidator<CalculateCartTotalQuery>
{
    public CalculateCartTotalQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
