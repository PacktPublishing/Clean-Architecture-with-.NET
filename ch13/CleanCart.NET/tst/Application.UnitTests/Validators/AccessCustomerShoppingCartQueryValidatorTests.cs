using Application.Operations.UseCases.AccessCustomerData;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

public class AccessCustomerShoppingCartQueryValidatorTests
{
    private readonly AccessCustomerShoppingCartQueryValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var query = new AccessCustomerShoppingCartQuery(Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.CustomerUserId)
            .WithErrorMessage("User ID cannot be empty.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        var query = new AccessCustomerShoppingCartQuery(Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerUserId);
    }
}