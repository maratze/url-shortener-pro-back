public class TwoFactorAuthResponse
{
    /// <summary>
    /// Статус двухфакторной аутентификации после операции
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// URL для QR-кода (если 2FA была настроена)
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Данные для QR-кода (otpauth URI)
    /// </summary>
    public string? QrCodeData { get; set; }

    /// <summary>
    /// Ключ для ручного ввода в приложение аутентификатора
    /// </summary>
    public string? ManualEntryKey { get; set; }

    /// <summary>
    /// Сообщение о результате операции
    /// </summary>
    public string? Message { get; set; }
} 