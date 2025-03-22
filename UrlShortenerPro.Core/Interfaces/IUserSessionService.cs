using UrlShortenerPro.Core.Dtos;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUserSessionService
{
    /// <summary>
    /// Получает все активные сессии текущего пользователя
    /// </summary>
    Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync();
    
    /// <summary>
    /// Получает текущую сессию пользователя
    /// </summary>
    Task<UserSessionDto> GetCurrentSessionAsync();
    
    /// <summary>
    /// Завершает указанную сессию пользователя по её идентификатору
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии</param>
    /// <returns>true, если сессия успешно завершена, иначе false</returns>
    Task<bool> TerminateSessionAsync(int sessionId);
    
    /// <summary>
    /// Завершает все активные сессии пользователя, кроме текущей
    /// </summary>
    /// <returns>Количество завершённых сессий</returns>
    Task<int> TerminateAllSessionsExceptCurrentAsync();
    
    /// <summary>
    /// Обновляет время последней активности для текущей сессии
    /// </summary>
    Task UpdateSessionActivityAsync();
} 