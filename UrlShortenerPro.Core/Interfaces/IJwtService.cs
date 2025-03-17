using System.Security.Claims;
using UrlShortenerPro.Core.Dtos;

namespace UrlShortenerPro.Core.Interfaces;

public interface IJwtService
{
    string GenerateToken(UserDto user);
    ClaimsPrincipal? GetPrincipalFromToken(string token);
    int? GetUserIdFromToken(string token);
    string? GetEmailFromToken(string token);
    bool ValidateToken(string token);
}