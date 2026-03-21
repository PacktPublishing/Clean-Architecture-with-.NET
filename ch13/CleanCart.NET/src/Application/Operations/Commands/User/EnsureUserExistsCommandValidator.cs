using FluentValidation;

namespace Application.Operations.Commands.User;

public class EnsureUserExistsCommandValidator : AbstractValidator<EnsureUserExistsCommand>
{
    public EnsureUserExistsCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(256);
    }
}