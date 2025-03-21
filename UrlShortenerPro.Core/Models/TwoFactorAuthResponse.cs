using System.Text.Json.Serialization;

namespace UrlShortenerPro.Core.Models;

public class TwoFactorAuthResponse
{
    /// <summary>
    /// Включена ли двухфакторная аутентификация
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// URL для QR-кода, который нужно отсканировать в приложении аутентификатора
    /// Заполняется только при первом шаге включения 2FA
    /// </summary>
    [JsonPropertyName("qrCodeUrl")]
    public string? QrCodeUrl { get; set; }
    
    /// <summary>
    /// Секретный ключ для ручного ввода (если QR-код не работает)
    /// Заполняется только при первом шаге включения 2FA
    /// </summary>
    [JsonPropertyName("manualEntryKey")]
    public string? ManualEntryKey { get; set; }
    
    /// <summary>
    /// Сообщение о статусе операции
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
} 