using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private const string RefreshTokenCookie = "refresh_token";

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);
        if (result.IsSuccess && result.Data?.RefreshToken is not null)
        {
            SetRefreshTokenCookie(result.Data.RefreshToken);
            result.Data.RefreshToken = null;
        }
        return result.ToActionResult(this);
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        if (result.IsSuccess && result.Data?.RefreshToken is not null)
        {
            SetRefreshTokenCookie(result.Data.RefreshToken);
            result.Data.RefreshToken = null;
        }
        return result.ToActionResult(this);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var token = Request.Cookies[RefreshTokenCookie]
            ?? (Request.ContentType?.Contains("application/json") == true
                ? (await ReadBodyRefreshTokenAsync())
                : null);

        if (string.IsNullOrEmpty(token))
            return Unauthorized(new { error = "Refresh token not provided." });

        var result = await authService.RefreshTokenAsync(token, ct);
        if (result.IsSuccess && result.Data?.RefreshToken is not null)
        {
            SetRefreshTokenCookie(result.Data.RefreshToken);
            result.Data.RefreshToken = null;
        }
        return result.ToActionResult(this);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = Request.Cookies[RefreshTokenCookie] ?? string.Empty;
        var result = await authService.LogoutAsync(token, ct);
        Response.Cookies.Delete(RefreshTokenCookie);
        return result.ToActionResult(this);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token, CancellationToken ct)
        => (await authService.ConfirmEmailAsync(userId, token, ct)).ToActionResult(this);

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
        => (await authService.ForgotPasswordAsync(request, ct)).ToActionResult(this);

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
        => (await authService.ResetPasswordAsync(request, ct)).ToActionResult(this);

    private void SetRefreshTokenCookie(string token)
    {
        Response.Cookies.Append(RefreshTokenCookie, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    private async Task<string?> ReadBodyRefreshTokenAsync()
    {
        Request.EnableBuffering();
        using var reader = new System.IO.StreamReader(Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        Request.Body.Position = 0;
        try
        {
            var json = System.Text.Json.JsonDocument.Parse(body);
            return json.RootElement.TryGetProperty("refreshToken", out var prop) ? prop.GetString() : null;
        }
        catch { return null; }
    }
}
