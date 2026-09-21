using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponse> RegisterAsync(RegisterRequest request);

        Task<TokenResponse?> LoginAsync(LoginRequest request);

        Task<TokenResponse?> RefreshTokenAsync(string refreshToken);
    }
}
