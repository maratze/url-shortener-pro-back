using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Api.Services;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public int? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        
        return null;
    }
    
    public string? GetCurrentToken()
    {
        // Получаем Authorization заголовок
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        
        // Если заголовок существует и содержит Bearer токен
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // Извлекаем токен
            return authHeader.Substring("Bearer ".Length).Trim();
        }
        
        return null;
    }
} 