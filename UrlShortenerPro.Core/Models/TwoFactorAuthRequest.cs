using System.Text.Json.Serialization;

namespace UrlShortenerPro.Core.Models;

public class TwoFactorAuthRequest
{
    /// <summary>
    /// True - для включения 2FA, False - для отключения
    /// </summary>
    [JsonPropertyName("enable")]
    public bool Enable { get; set; }
    
    /// <summary>
    /// Код подтверждения из приложения аутентификатора (при включении или отключении)
    /// null - при первом шаге включения, чтобы получить QR-код
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }
} 