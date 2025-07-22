using FluentValidation;

namespace Application.Operations.UseCases.AccessCustomerData;

public class AccessCustomerCartQueryValidator : AbstractValidator<AccessCustomerCartQuery>
{
    public AccessCustomerCartQueryValidator()
    {
        RuleFor(x => x.AuthorizationUserId).NotEmpty();
        RuleFor(x => x.CustomerUserId).NotEmpty();
    }
}
