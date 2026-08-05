using FluentAssertions;
using FluentValidation;
using SchoolCRM.Application.DTOs.Auth;
using SchoolCRM.Application.Validators.Auth;
using Xunit;

namespace SchoolCRM.Tests.Unit.Validators;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator;

    public LoginValidatorTests()
    {
        _validator = new LoginValidator();
    }

    [Fact]
    public void Valid_Login_DoesNotThrow()
    {
        var dto = new LoginDto
        {
            Email = "john.doe@test.com",
            Password = "Password@123"
        };

        var act = () => _validator.Validate(dto);

        var result = act();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Empty_Email_ThrowsValidationException()
    {
        var dto = new LoginDto
        {
            Email = "",
            Password = "Password@123"
        };

        var act = () => _validator.ValidateAndThrow(dto);

        act.Should().Throw<ValidationException>()
            .Which.Errors.Should().Contain(e =>
                e.PropertyName == nameof(LoginDto.Email) &&
                e.ErrorMessage == "Email is required");
    }

    [Fact]
    public void Invalid_Email_ThrowsValidationException()
    {
        var dto = new LoginDto
        {
            Email = "not-an-email",
            Password = "Password@123"
        };

        var act = () => _validator.ValidateAndThrow(dto);

        act.Should().Throw<ValidationException>()
            .Which.Errors.Should().Contain(e =>
                e.PropertyName == nameof(LoginDto.Email) &&
                e.ErrorMessage == "Invalid email address");
    }

    [Fact]
    public void Empty_Password_ThrowsValidationException()
    {
        var dto = new LoginDto
        {
            Email = "john.doe@test.com",
            Password = ""
        };

        var act = () => _validator.ValidateAndThrow(dto);

        act.Should().Throw<ValidationException>()
            .Which.Errors.Should().Contain(e =>
                e.PropertyName == nameof(LoginDto.Password) &&
                e.ErrorMessage == "Password is required");
    }

    [Fact]
    public void Short_Password_ThrowsValidationException()
    {
        var dto = new LoginDto
        {
            Email = "john.doe@test.com",
            Password = "12345"
        };

        var act = () => _validator.ValidateAndThrow(dto);

        act.Should().Throw<ValidationException>()
            .Which.Errors.Should().Contain(e =>
                e.PropertyName == nameof(LoginDto.Password) &&
                e.ErrorMessage == "Password must be at least 6 characters");
    }

    [Fact]
    public void Null_Email_ThrowsValidationException()
    {
        var dto = new LoginDto
        {
            Email = null!,
            Password = "Password@123"
        };

        var act = () => _validator.ValidateAndThrow(dto);

        act.Should().Throw<ValidationException>()
            .Which.Errors.Should().Contain(e =>
                e.PropertyName == nameof(LoginDto.Email));
    }

    [Fact]
    public void Both_Invalid_ReturnsMultipleErrors()
    {
        var dto = new LoginDto
        {
            Email = "invalid-email",
            Password = ""
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginDto.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginDto.Password));
    }

    [Fact]
    public void Null_Password_ThrowsValidationException()
    {
        var dto = new LoginDto
        {
            Email = "john.doe@test.com",
            Password = null!
        };

        var act = () => _validator.ValidateAndThrow(dto);

        act.Should().Throw<ValidationException>()
            .Which.Errors.Should().Contain(e =>
                e.PropertyName == nameof(LoginDto.Password) &&
                e.ErrorMessage == "Password is required");
    }
}
