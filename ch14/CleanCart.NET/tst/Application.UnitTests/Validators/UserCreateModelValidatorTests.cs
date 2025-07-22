using Application.Operations.Commands.User;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

public class UserCreateModelValidatorTests
{
    private readonly UserCreateModelValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        var model = new UserCreateModel { Username = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var model = new UserCreateModel { Email = "invalidemail" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_FullName_Is_Empty()
    {
        var model = new UserCreateModel { FullName = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Should_Have_Error_When_Roles_Are_Empty()
    {
        var model = new UserCreateModel { Roles = new List<UserRole>() };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Roles);
    }

    [Fact]
    public void Should_Have_Error_When_Role_Is_Invalid()
    {
        var model = new UserCreateModel { Roles = new List<UserRole> { (UserRole)999 } };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Roles);
    }
}