using FluentValidation;

namespace Application.Operations.UseCases.AccessCustomerData;

public class AccessCustomerOrderHistoryQueryValidator : AbstractValidator<AccessCustomerOrderHistoryQuery>
{
    public AccessCustomerOrderHistoryQueryValidator()
    {
        RuleFor(x => x.AuthorizationUserId).NotEmpty();
        RuleFor(x => x.CustomerUserId).NotEmpty();
    }
}
