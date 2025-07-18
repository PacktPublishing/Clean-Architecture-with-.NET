using Application.Operations.UseCases.RemoveItemFromCart;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

public class RemoveItemFromCartCommandValidatorTests
{
    private readonly RemoveItemFromCartCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var model = new RemoveItemFromCartCommand(Guid.Empty, Guid.NewGuid(), 1);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Have_Error_When_ProductId_Is_Empty()
    {
        var model = new RemoveItemFromCartCommand(Guid.NewGuid(), Guid.Empty, 1);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Should_Have_Error_When_Quantity_Is_Less_Than_Or_Equal_To_Zero()
    {
        var model = new RemoveItemFromCartCommand(Guid.NewGuid(), Guid.NewGuid(), 0);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Input_Is_Valid()
    {
        var model = new RemoveItemFromCartCommand(Guid.NewGuid(), Guid.NewGuid(), 1);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }
}
