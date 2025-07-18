using Application.Operations.UseCases.AccessCustomerData;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

public class AccessCustomerOrderHistoryQueryValidatorTests
{
    private readonly AccessCustomerOrderHistoryQueryValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_AuthorizationUserId_Is_Empty()
    {
        var model = new AccessCustomerOrderHistoryQuery(Guid.Empty, Guid.NewGuid());
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.AuthorizationUserId);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerUserId_Is_Empty()
    {
        var model = new AccessCustomerOrderHistoryQuery(Guid.NewGuid(), Guid.Empty);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CustomerUserId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Ids_Are_Valid()
    {
        var model = new AccessCustomerOrderHistoryQuery(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.AuthorizationUserId);
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerUserId);
    }
}
