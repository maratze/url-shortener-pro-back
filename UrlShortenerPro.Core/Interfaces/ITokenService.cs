using UrlShortenerPro.Core.Dtos;

namespace UrlShortenerPro.Core.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(UserDto user, string deviceInfo, string ipAddress, string location);
    Task<bool> ValidateTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
    Task<UserSessionDto?> GetSessionByTokenAsync(string token);
} 