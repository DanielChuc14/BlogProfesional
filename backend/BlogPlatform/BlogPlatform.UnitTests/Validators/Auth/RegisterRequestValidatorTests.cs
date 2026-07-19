using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Validators.Auth;
using FluentAssertions;

namespace BlogPlatform.UnitTests.Validators.Auth;

/// <summary>
/// Pruebas del RegisterRequestValidator.
///
/// Los validators son lógica pura sin dependencias externas, así que se instancian
/// directamente sin ningún mock.
///
/// [Fact] = un solo caso de prueba fijo.
/// [Theory] + [InlineData] = el mismo test ejecutado con múltiples inputs. Ideal para
/// validar muchas variantes inválidas sin duplicar código.
/// </summary>
public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    // ── Caso exitoso ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password1",
            DisplayName = "Test User",
            Username = "test_user"
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    // ── Email ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]           // vacío
    [InlineData("notanemail")] // sin @
    [InlineData("missing@")]   // dominio vacío
    public async Task InvalidEmail_ShouldFailWithEmailError(string email)
    {
        var request = new RegisterRequest
        {
            Email = email,
            Password = "Password1",
            DisplayName = "Test User",
            Username = "testuser"
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // ── Password ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Abc123")]       // menos de 8 chars
    [InlineData("alllower1")]    // sin mayúscula
    [InlineData("NoDigitHere")]  // sin dígito
    public async Task InvalidPassword_ShouldFailWithPasswordError(string password)
    {
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = password,
            DisplayName = "Test User",
            Username = "testuser"
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // ── Username ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("ab")]          // menos de 3 chars
    [InlineData("user name")]   // espacio no permitido
    [InlineData("user@name")]   // @ no permitido
    [InlineData("user.name")]   // punto no permitido
    public async Task InvalidUsername_ShouldFailWithUsernameError(string username)
    {
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password1",
            DisplayName = "Test User",
            Username = username
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    // ── DisplayName ───────────────────────────────────────────────────────────

    [Fact]
    public async Task EmptyDisplayName_ShouldFailValidation()
    {
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password1",
            DisplayName = "X", // menos de 2 chars
            Username = "testuser"
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }
}
