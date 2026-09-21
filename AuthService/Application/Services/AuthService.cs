using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<TokenResponse> RegisterAsync(
            RegisterRequest request)
        {
            // 1. Check whether user already exists
            var existingUser = await _userRepository
                .GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "A user with this email already exists.");
            }

            // 2. Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email.Trim().ToLowerInvariant(),
                Role = Enum.Parse<UserRole>(request.Role, true),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Hash password
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            // 4. Save user
            await _userRepository.AddAsync(user);

            // 5. Generate tokens
            var accessToken = _tokenService
                .GenerateAccessToken(user);

            var refreshToken = _tokenService
                .GenerateRefreshToken();

            // 6. Store refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _userRepository
                .AddRefreshTokenAsync(refreshTokenEntity);

            await _userRepository.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt =
                    _tokenService.GetAccessTokenExpiration()
            };
        }

        public async Task<TokenResponse?> LoginAsync(
            LoginRequest request)
        {
            // 1. Find user
            var user = await _userRepository
                .GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

            if (user == null)
                return null;

            // 2. Check account status
            if (!user.IsActive)
                return null;

            // 3. Verify password
            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
                return null;

            // 4. Update last login
            user.LastLoginAt = DateTime.UtcNow;

            // 5. Generate tokens
            var accessToken = _tokenService
                .GenerateAccessToken(user);

            var refreshToken = _tokenService
                .GenerateRefreshToken();

            // 6. Store refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _userRepository
                .AddRefreshTokenAsync(refreshTokenEntity);

            await _userRepository.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiration(),
                User = new UserInfo
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    IsActive = user.IsActive
                }
            };
        }

        public async Task<TokenResponse?> RefreshTokenAsync(
            string refreshToken)
        {
            // 1. Find refresh token
            var storedToken = await _userRepository
                .GetRefreshTokenAsync(refreshToken);

            if (storedToken == null)
                return null;

            // 2. Check whether token is revoked
            if (storedToken.IsRevoked)
                return null;

            // 3. Check expiration
            if (storedToken.ExpiresAt <= DateTime.UtcNow)
                return null;

            // 4. Check user
            if (!storedToken.User.IsActive)
                return null;

            // 5. Revoke old refresh token
            storedToken.IsRevoked = true;

            // 6. Generate new tokens
            var accessToken = _tokenService
                .GenerateAccessToken(storedToken.User);

            var newRefreshToken = _tokenService
                .GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = newRefreshToken,
                UserId = storedToken.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _userRepository
                .AddRefreshTokenAsync(newRefreshTokenEntity);

            await _userRepository.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt =
                    _tokenService.GetAccessTokenExpiration()
            };
        }
    }
}
