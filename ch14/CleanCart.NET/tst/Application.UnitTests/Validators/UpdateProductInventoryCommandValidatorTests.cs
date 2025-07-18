using Application.Operations.UseCases.ManageProductInventory;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

public class UpdateProductInventoryCommandValidatorTests
{
    private readonly UpdateProductInventoryCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var model = new UpdateProductInventoryCommand(Guid.Empty, Guid.NewGuid(), 10);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Have_Error_When_ProductId_Is_Empty()
    {
        var model = new UpdateProductInventoryCommand(Guid.NewGuid(), Guid.Empty, 10);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Should_Have_Error_When_StockLevel_Is_Negative()
    {
        var model = new UpdateProductInventoryCommand(Guid.NewGuid(), Guid.NewGuid(), -1);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.StockLevel);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Input_Is_Valid()
    {
        var model = new UpdateProductInventoryCommand(Guid.NewGuid(), Guid.NewGuid(), 10);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
