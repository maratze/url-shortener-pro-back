namespace UrlShortenerPro.Core.Models;

/// <summary>
/// Модель сессии пользователя для использования в Core
/// </summary>
public class UserSession
{
    /// <summary>
    /// Идентификатор сессии
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя, которому принадлежит сессия
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Информация об устройстве пользователя
    /// </summary>
    public string? DeviceInfo { get; set; }
    
    /// <summary>
    /// IP-адрес пользователя
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Географическое местоположение пользователя
    /// </summary>
    public string? Location { get; set; }
    
    /// <summary>
    /// Дата и время создания сессии
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Дата и время последней активности пользователя
    /// </summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// JWT токен сессии
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// Флаг активности сессии
    /// </summary>
    public bool IsActive { get; set; } = true;
} 