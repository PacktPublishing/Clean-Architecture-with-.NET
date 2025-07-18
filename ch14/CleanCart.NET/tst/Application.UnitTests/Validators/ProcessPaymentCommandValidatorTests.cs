using Application.Operations.UseCases.ProcessPayment;
using Domain.Entities;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

public class ProcessPaymentCommandValidatorTests
{
    private readonly ProcessPaymentCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var model = new ProcessPaymentCommand { UserId = Guid.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Have_Error_When_Items_Are_Empty()
    {
        var model = new ProcessPaymentCommand { Items = new List<ShoppingCartItem>() };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_Is_Invalid()
    {
        var model = new ProcessPaymentCommand { CardNumber = "1234" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CardNumber).WithErrorMessage("Invalid card number.");
    }

    [Fact]
    public void Should_Have_Error_When_CardHolderName_Is_Empty()
    {
        var model = new ProcessPaymentCommand { CardHolderName = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CardHolderName);
    }

    [Fact]
    public void Should_Have_Error_When_ExpirationMonthYear_Is_Invalid()
    {
        var model = new ProcessPaymentCommand { ExpirationMonthYear = "13/99" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ExpirationMonthYear).WithErrorMessage("Expiration must be in MM/YY format.");
    }

    [Fact]
    public void Should_Have_Error_When_CVV_Is_Invalid()
    {
        var model = new ProcessPaymentCommand { CVV = "12" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CVV).WithErrorMessage("CVV must be 3 or 4 digits.");
    }

    [Fact]
    public void Should_Have_Error_When_PostalCode_Is_Empty()
    {
        var model = new ProcessPaymentCommand { PostalCode = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PostalCode);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = new ProcessPaymentCommand
        {
            UserId = Guid.NewGuid(),
            Items = new List<ShoppingCartItem> { new ShoppingCartItem(Guid.NewGuid(), "Product", 10, 1) },
            CardNumber = "4111111111111111",
            CardHolderName = "John Doe",
            ExpirationMonthYear = "12/25",
            CVV = "123",
            PostalCode = "12345"
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
