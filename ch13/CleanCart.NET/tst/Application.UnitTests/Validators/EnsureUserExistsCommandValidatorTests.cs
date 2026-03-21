using Application.Operations.Commands.User;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

public class EnsureUserExistsCommandValidatorTests
{
    private readonly EnsureUserExistsCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        var command = new EnsureUserExistsCommand
        {
            Username = string.Empty,
            Email = "user@example.com",
            FullName = "Test User"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var command = new EnsureUserExistsCommand
        {
            Username = "user@example.com",
            Email = string.Empty,
            FullName = "Test User"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new EnsureUserExistsCommand
        {
            Username = "user@example.com",
            Email = "invalid-email",
            FullName = "Test User"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_FullName_Is_Empty()
    {
        var command = new EnsureUserExistsCommand
        {
            Username = "user@example.com",
            Email = "user@example.com",
            FullName = string.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new EnsureUserExistsCommand
        {
            Username = "user@example.com",
            Email = "user@example.com",
            FullName = "Test User"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }
}