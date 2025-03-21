namespace UrlShortenerPro.Core.Models.Auth
{
    /// <summary>
    /// Запрос на включение/отключение двухфакторной аутентификации
    /// </summary>
    public class TwoFactorAuthRequest
    {
        /// <summary>
        /// Если true - включить 2FA, если false - отключить
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Код верификации из приложения аутентификатора
        /// </summary>
        public string? Code { get; set; }
    }
} 