using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Application.Interfaces;
using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities.Auth;
using BlogPlatform.Domain.Entities.Content;
using BlogPlatform.Domain.Interfaces;
using BlogPlatform.Infrastructure.Helpers;
using FluentValidation;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BlogPlatform.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IUnitOfWork uow,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AuthService> logger,
    IHttpContextAccessor httpContextAccessor,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<ForgotPasswordRequest> forgotPasswordValidator,
    IValidator<ResetPasswordRequest> resetPasswordValidator) : IAuthService
{
    public async Task<ResultModel<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        await registerValidator.ValidateAndThrowAsync(request, ct);

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return ResultModel<AuthResponse>.Conflict("Email is already registered.");

        var existingUsername = await userManager.FindByNameAsync(request.Username);
        if (existingUsername is not null)
            return ResultModel<AuthResponse>.Conflict("Username is already taken.");

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Username,
            DisplayName = request.DisplayName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ResultModel<AuthResponse>.BadRequest(errors);
        }

        await userManager.AddToRoleAsync(user, "Reader");

        var profileSlug = SlugHelper.Generate(request.Username);
        await uow.BlogProfiles.AddAsync(new BlogProfile
        {
            UserId = user.Id,
            Slug = profileSlug
        }, ct);
        await uow.SaveChangesAsync(ct);

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailService.SendEmailConfirmationAsync(user.Email!, token, ct);
        await emailService.SendWelcomeAsync(user.Email!, user.DisplayName, ct);

        logger.LogInformation("New user registered: {UserId} ({Email})", user.Id, user.Email);

        var response = await BuildAuthResponseAsync(user, ct);
        return ResultModel<AuthResponse>.Created(response);
    }

    public async Task<ResultModel<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        await loginValidator.ValidateAndThrowAsync(request, ct);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return ResultModel<AuthResponse>.Unauthorized("Invalid credentials.");

        if (!user.IsActive)
            return ResultModel<AuthResponse>.Forbidden("Account is disabled.");

        if (user.SuspendedUntil.HasValue && user.SuspendedUntil.Value > DateTime.UtcNow)
            return ResultModel<AuthResponse>.Forbidden(
                $"Account is suspended until {user.SuspendedUntil.Value:yyyy-MM-dd HH:mm} UTC.");

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return ResultModel<AuthResponse>.Unauthorized("Invalid credentials.");

        if (!user.EmailConfirmed)
            return ResultModel<AuthResponse>.Unauthorized("Email is not confirmed.");

        var response = await BuildAuthResponseAsync(user, ct);
        return ResultModel<AuthResponse>.Ok(response);
    }

    public async Task<ResultModel<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await uow.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.Token == tokenHash, ct);

        if (stored is null || !stored.IsActive)
            return ResultModel<AuthResponse>.Unauthorized("Invalid or expired refresh token.");

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null || !user.IsActive)
            return ResultModel<AuthResponse>.Unauthorized("User not found or inactive.");

        stored.IsRevoked = true;
        stored.RevokedReason = "Replaced by new token";
        uow.RefreshTokens.Update(stored);

        var response = await BuildAuthResponseAsync(user, ct);
        return ResultModel<AuthResponse>.Ok(response);
    }

    public async Task<ResultModel> LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await uow.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.Token == tokenHash, ct);

        if (stored is null)
            return ResultModel.NoContent();

        stored.IsRevoked = true;
        stored.RevokedReason = "Logout";
        uow.RefreshTokens.Update(stored);
        await uow.SaveChangesAsync(ct);

        return ResultModel.NoContent();
    }

    public async Task<ResultModel> ConfirmEmailAsync(string userId, string token, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return ResultModel.NotFound("User not found.");

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ResultModel.BadRequest(errors);
        }

        return ResultModel.NoContent();
    }

    public async Task<ResultModel> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        await forgotPasswordValidator.ValidateAndThrowAsync(request, ct);

        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is not null && user.EmailConfirmed)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await emailService.SendPasswordResetAsync(user.Email!, token, ct);
        }

        return ResultModel.NoContent();
    }

    public async Task<ResultModel> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        await resetPasswordValidator.ValidateAndThrowAsync(request, ct);

        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return ResultModel.BadRequest("Invalid request.");

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ResultModel.BadRequest(errors);
        }

        return ResultModel.NoContent();
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user, CancellationToken ct)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, ct);

        var expiresIn = int.Parse(configuration["Jwt:AccessTokenMinutes"] ?? "15") * 60;

        return new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            UserId = user.Id,
            Email = user.Email!,
            Username = user.UserName!,
            DisplayName = user.DisplayName,
            Roles = roles,
            RefreshToken = refreshToken,
            AvatarUrl = user.AvatarUrl,
            PreferredLanguage = user.PreferredLanguage
        };
    }

    private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("display_name", user.DisplayName)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var minutes = int.Parse(configuration["Jwt:AccessTokenMinutes"] ?? "15");
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<ResultModel<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken ct = default)
    {
        var clientId = configuration["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId) || clientId == "PENDIENTE")
            return ResultModel<AuthResponse>.BadRequest("Google OAuth is not configured.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException)
        {
            return ResultModel<AuthResponse>.Unauthorized("Invalid Google token.");
        }

        var user = await userManager.FindByEmailAsync(payload.Email);

        if (user is not null)
        {
            if (!user.IsActive)
                return ResultModel<AuthResponse>.Forbidden("Account is disabled.");

            var logins = await userManager.GetLoginsAsync(user);
            if (!logins.Any(l => l.LoginProvider == "Google"))
                await userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google"));
        }
        else
        {
            user = new ApplicationUser
            {
                Email = payload.Email,
                UserName = payload.Email.Split('@')[0],
                DisplayName = payload.Name ?? payload.Email.Split('@')[0],
                EmailConfirmed = true,
                AvatarUrl = payload.Picture,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return ResultModel<AuthResponse>.BadRequest(errors);
            }

            await userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google"));
            await userManager.AddToRoleAsync(user, "Reader");

            var profileSlug = SlugHelper.Generate(user.UserName!);
            await uow.BlogProfiles.AddAsync(new BlogProfile { UserId = user.Id, Slug = profileSlug }, ct);
            await uow.SaveChangesAsync(ct);

            logger.LogInformation("New user registered via Google: {UserId} ({Email})", user.Id, user.Email);
        }

        var response = await BuildAuthResponseAsync(user, ct);
        return ResultModel<AuthResponse>.Ok(response);
    }

    private async Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken ct)
    {
        const int maxActiveTokens = 5;
        var days = int.Parse(configuration["Jwt:RefreshTokenDays"] ?? "7");

        var activeTokens = await uow.RefreshTokens.FindAsync(
            rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow, ct);

        var activeList = activeTokens.OrderBy(rt => rt.ExpiresAt).ToList();
        if (activeList.Count >= maxActiveTokens)
        {
            var oldest = activeList.First();
            oldest.IsRevoked = true;
            oldest.RevokedReason = "Exceeded max active tokens";
            uow.RefreshTokens.Update(oldest);
        }

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = HashToken(rawToken);

        var deviceInfo = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

        var refreshToken = new RefreshToken
        {
            Token = tokenHash,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(days),
            DeviceInfo = deviceInfo?.Length > 256 ? deviceInfo[..256] : deviceInfo
        };

        await uow.RefreshTokens.AddAsync(refreshToken, ct);
        await uow.SaveChangesAsync(ct);

        return rawToken;
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
