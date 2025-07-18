using Application.Operations.Queries.User;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

public class GetUserByUsernameQueryValidatorTests
{
    private readonly GetUserByUsernameQueryValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        var query = new GetUserByUsernameQuery(string.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Username_Exceeds_MaxLength()
    {
        var query = new GetUserByUsernameQuery(new string('a', 51));
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username must not exceed 50 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Username_Is_Valid()
    {
        var query = new GetUserByUsernameQuery("ValidUsername");
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }
}
