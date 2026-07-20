using BlogPlatform.Application.DTOs.Auth;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface IAuthService
{
    Task<ResultModel<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<ResultModel<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ResultModel<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<ResultModel> LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<ResultModel> ConfirmEmailAsync(string userId, string token, CancellationToken ct = default);
    Task<ResultModel> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task<ResultModel> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
}
