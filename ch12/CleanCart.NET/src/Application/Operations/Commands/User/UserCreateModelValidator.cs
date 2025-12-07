using Domain.Enums;
using FluentValidation;

namespace Application.Operations.Commands.User;

public class UserCreateModelValidator : AbstractValidator<UserCreateModel>
{
    public UserCreateModelValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .Length(3, 100).WithMessage("Full name must be between 3 and 100 characters.");

        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("At least one role is required.")
            .Must(roles => roles.All(role => Enum.IsDefined(typeof(UserRole), role)))
            .WithMessage("Invalid role specified.")
            .Must(roles => roles.Count <= 4000).WithMessage("Roles must not exceed 4000 characters.");
    }
}