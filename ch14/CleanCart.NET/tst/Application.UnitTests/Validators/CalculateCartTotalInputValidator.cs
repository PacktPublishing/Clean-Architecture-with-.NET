using Application.Operations.UseCases.CalculateCartTotal;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

public class CalculateCartTotalQueryValidatorTests
{
    private readonly CalculateCartTotalQueryValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var model = new CalculateCartTotalQuery(Guid.Empty);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Provided()
    {
        var model = new CalculateCartTotalQuery(Guid.NewGuid());
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}