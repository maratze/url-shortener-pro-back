namespace UrlShortenerPro.Core.Interfaces;

public interface ICurrentUserProvider
{
    /// <summary>
    /// Получает идентификатор текущего пользователя
    /// </summary>
    /// <returns>Идентификатор пользователя или null, если пользователь не аутентифицирован</returns>
    int? GetCurrentUserId();
    
    /// <summary>
    /// Получает токен текущей сессии пользователя
    /// </summary>
    /// <returns>Токен сессии или null, если пользователь не аутентифицирован</returns>
    string? GetCurrentToken();
} 