using System.Security.Claims;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Core.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    ClaimsPrincipal? GetPrincipalFromToken(string token);
}