using System.Linq.Expressions;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Application.Validators.Auth;
using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Interfaces;
using BlogPlatform.Infrastructure.Services;
using BlogPlatform.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BlogPlatform.UnitTests.Services.Auth;

/// <summary>
/// Pruebas unitarias de AuthService.
///
/// Un "mock" (o substitute con NSubstitute) es un objeto falso que simula el
/// comportamiento de una dependencia real. En lugar de conectar a una base de datos
/// real, le decimos al mock: "cuando alguien llame a FindByEmailAsync, devuelve esto".
///
/// Esto permite testear la lógica del service de forma aislada, sin depender de la DB,
/// del email server, ni de ningún otro sistema externo.
/// </summary>
public class AuthServiceTests
{
    // ── Dependencias mockeadas ────────────────────────────────────────────────
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;

    // ── El servicio bajo test (el real) ──────────────────────────────────────
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userManager = UserManagerFactory.Create();
        _uow = Substitute.For<IUnitOfWork>();
        _emailService = Substitute.For<IEmailService>();

        // IConfiguration necesita valores reales para que GenerateAccessToken funcione.
        // AddInMemoryCollection es la forma estándar de configurar valores en tests.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-must-be-32-chars-minimum!!",
                ["Jwt:Issuer"] = "blogplatform-test",
                ["Jwt:Audience"] = "blogplatform-test",
                ["Jwt:AccessTokenMinutes"] = "15",
                ["Jwt:RefreshTokenDays"] = "7"
            })
            .Build();

        // Los validators se usan reales — son lógica pura que ya testeamos por separado.
        var registerValidator = new RegisterRequestValidator();
        var loginValidator = new LoginRequestValidator();
        var forgotPasswordValidator = new ForgotPasswordRequestValidator();
        var resetPasswordValidator = new ResetPasswordRequestValidator();

        // ILogger no necesita verificaciones en estos tests — lo mockeamos para que no falle.
        var logger = Substitute.For<ILogger<AuthService>>();

        // HttpContext se necesita para que CreateRefreshTokenAsync lea el User-Agent.
        var httpContext = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        request.Headers.Returns(new HeaderDictionary { ["User-Agent"] = "xunit-test" });
        httpContext.Request.Returns(request);
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        // ── Setup del IUnitOfWork ─────────────────────────────────────────────
        // Configuramos los mocks del UoW con comportamientos por defecto.
        // Cada test puede sobreescribir estos si necesita un comportamiento distinto.

        // FindAsync: para CreateRefreshTokenAsync — devuelve lista vacía (sin tokens activos previos)
        _uow.RefreshTokens
            .FindAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<RefreshToken>());

        // AddAsync: no hace nada (simula inserción en DB)
        _uow.RefreshTokens
            .AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _uow.BlogProfiles
            .AddAsync(Arg.Any<BlogProfile>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // SaveChangesAsync: simula 1 fila afectada
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        _authService = new AuthService(
            _userManager,
            _uow,
            _emailService,
            configuration,
            logger,
            httpContextAccessor,
            registerValidator,
            loginValidator,
            forgotPasswordValidator,
            resetPasswordValidator);
    }

    // ── RegisterAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ReturnsConflict()
    {
        // Arrange: configuramos el mock para que devuelva un usuario existente con ese email.
        var existingUser = new ApplicationUser { Email = "taken@test.com" };
        _userManager.FindByEmailAsync("taken@test.com").Returns(existingUser);

        var request = new RegisterRequest
        {
            Email = "taken@test.com",
            Password = "Password1",
            DisplayName = "Test User",
            Username = "newusername"
        };

        // Act: ejecutamos el método bajo test.
        var result = await _authService.RegisterAsync(request);

        // Assert: verificamos el resultado con FluentAssertions.
        // Should().BeFalse() es más legible que Assert.False(result.IsSuccess)
        // y da mensajes de error más descriptivos cuando falla.
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409); // Conflict
        result.Error.Should().Contain("Email is already registered");
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameAlreadyExists_ReturnsConflict()
    {
        // Email libre, username tomado
        _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);
        var existingUser = new ApplicationUser { UserName = "takenuser" };
        _userManager.FindByNameAsync("takenuser").Returns(existingUser);

        var result = await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "new@test.com",
            Password = "Password1",
            DisplayName = "Test User",
            Username = "takenuser"
        });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Error.Should().Contain("Username is already taken");
    }

    [Fact]
    public async Task RegisterAsync_HappyPath_ReturnsCreatedWithReaderRole()
    {
        // Arrange: todos los checks pasan, el usuario se crea correctamente.
        _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);
        _userManager.FindByNameAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                    .Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "Reader")
                    .Returns(IdentityResult.Success);
        _userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>())
                    .Returns("fake-confirmation-token");
        _userManager.GetRolesAsync(Arg.Any<ApplicationUser>())
                    .Returns(new List<string> { "Reader" });
        _emailService.SendEmailConfirmationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                     .Returns(Task.CompletedTask);
        _emailService.SendWelcomeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                     .Returns(Task.CompletedTask);

        var result = await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "newuser@test.com",
            Password = "Password1",
            DisplayName = "New User",
            Username = "newuser"
        });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201); // Created
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be("newuser@test.com");
        result.Data.Roles.Should().Contain("Reader");
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
    }

    // ── LoginAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ReturnsUnauthorized()
    {
        _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "nobody@test.com",
            Password = "Password1"
        });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401); // Unauthorized
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ReturnsForbidden()
    {
        // IsActive = false simula una cuenta baneada/desactivada
        var inactiveUser = new ApplicationUser
        {
            Email = "user@test.com",
            IsActive = false
        };
        _userManager.FindByEmailAsync("user@test.com").Returns(inactiveUser);

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "user@test.com",
            Password = "Password1"
        });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403); // Forbidden
        result.Error.Should().Contain("Account is disabled");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsInvalid_ReturnsUnauthorized()
    {
        var user = new ApplicationUser
        {
            Email = "user@test.com",
            IsActive = true,
            EmailConfirmed = true
        };
        _userManager.FindByEmailAsync("user@test.com").Returns(user);
        // CheckPasswordAsync retorna false → contraseña incorrecta
        _userManager.CheckPasswordAsync(user, Arg.Any<string>()).Returns(false);

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "user@test.com",
            Password = "WrongPassword1"
        });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task LoginAsync_WhenEmailNotConfirmed_ReturnsUnauthorized()
    {
        var user = new ApplicationUser
        {
            Email = "user@test.com",
            IsActive = true,
            EmailConfirmed = false // email sin confirmar
        };
        _userManager.FindByEmailAsync("user@test.com").Returns(user);
        _userManager.CheckPasswordAsync(user, Arg.Any<string>()).Returns(true);

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "user@test.com",
            Password = "Password1"
        });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Error.Should().Contain("Email is not confirmed");
    }
}
