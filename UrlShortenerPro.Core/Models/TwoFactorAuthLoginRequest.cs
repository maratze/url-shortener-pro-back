namespace UrlShortenerPro.Core.Models;

/// <summary>
/// Запрос на проверку кода двухфакторной аутентификации при входе в систему
/// </summary>
public class TwoFactorAuthLoginRequest
{
    /// <summary>
    /// Email пользователя
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Код верификации из приложения-аутентификатора
    /// </summary>
    public string VerificationCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Опция "запомнить меня" для долгосрочного хранения токена
    /// </summary>
    public bool Remember { get; set; } = false;
} 