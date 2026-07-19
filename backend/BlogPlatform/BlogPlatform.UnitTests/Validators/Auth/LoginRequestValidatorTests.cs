using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Validators.Auth;
using FluentAssertions;

namespace BlogPlatform.UnitTests.Validators.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var result = await _validator.ValidateAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "anypassword"
        });

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    public async Task InvalidEmail_ShouldFailValidation(string email)
    {
        var result = await _validator.ValidateAsync(new LoginRequest
        {
            Email = email,
            Password = "anypassword"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task EmptyPassword_ShouldFailValidation()
    {
        var result = await _validator.ValidateAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = ""
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
