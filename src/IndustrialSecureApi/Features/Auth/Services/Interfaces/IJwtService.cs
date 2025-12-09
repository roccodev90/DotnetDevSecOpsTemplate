using System.Security.Claims;
using IndustrialSecureApi.Infrastructure;

namespace IndustrialSecureApi.Features.Auth.Services.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    Task<string> SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    Task<UserRefreshToken?> GetRefreshTokenAsync(string refreshToken);
}