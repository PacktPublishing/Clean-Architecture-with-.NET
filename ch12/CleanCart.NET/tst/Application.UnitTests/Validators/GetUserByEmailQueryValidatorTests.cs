using Application.Operations.Queries.User;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

public class GetUserByEmailQueryValidatorTests
{
    private readonly GetUserByEmailQueryValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var query = new GetUserByEmailQuery(string.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var query = new GetUserByEmailQuery("invalid-email");
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Email_Is_Valid()
    {
        var query = new GetUserByEmailQuery("test@example.com");
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}