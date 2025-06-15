using FluentValidation;

namespace Application.Operations.Queries.User;

public class GetUserByUsernameQueryValidator : AbstractValidator<GetUserByUsernameQuery>
{
    public GetUserByUsernameQueryValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MaximumLength(50)
            .WithMessage("Username must not exceed 50 characters.");
    }
}