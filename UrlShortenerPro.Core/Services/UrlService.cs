using System.Text;
using Microsoft.Extensions.Configuration;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;
using UrlShortenerPro.Infrastructure.Interfaces;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Core.Services;

public class UrlService(
    IUrlRepository urlRepository,
    IClickDataRepository clickDataRepository,
    IUserRepository userRepository,
    IConfiguration configuration)
    : IUrlService
{
    private readonly Random _random = new();

    public async Task<UrlResponse> CreateShortUrlAsync(UrlCreationRequest request)
    {
        // Проверка лимитов для пользователя
        bool canCreate = await IsUserAllowedToCreateMoreUrlsAsync(request.UserId);
        if (!canCreate)
        {
            throw new InvalidOperationException("Превышен лимит создания ссылок для вашего тарифа");
        }

        // Генерация или проверка короткого кода
        string shortCode;
        if (!string.IsNullOrEmpty(request.CustomCode) && request.UserId.HasValue)
        {
            // Проверка, что пользователь премиум для использования кастомного кода
            if (!await IsUserPremiumAsync(request.UserId.Value))
            {
                throw new InvalidOperationException("Кастомные ссылки доступны только для премиум пользователей");
            }

            // Проверка доступности кастомного кода
            bool exists = await urlRepository.ShortCodeExistsAsync(request.CustomCode);
            if (exists)
            {
                throw new InvalidOperationException("Этот кастомный код уже занят");
            }

            shortCode = request.CustomCode;
        }
        else
        {
            // Генерация случайного кода
            shortCode = await GenerateUniqueShortCodeAsync();
        }

        // Расчет срока истечения
        DateTime? expiresAt = null;
        if (request.ExpiresInDays.HasValue && request.ExpiresInDays.Value > 0)
        {
            expiresAt = DateTime.UtcNow.AddDays(request.ExpiresInDays.Value);
        }
        else if (request.UserId.HasValue && await IsUserPremiumAsync(request.UserId.Value))
        {
            // Премиум пользователи могут иметь бессрочные ссылки
            expiresAt = null;
        }
        else
        {
            // По умолчанию для бесплатных - 30 дней
            expiresAt = DateTime.UtcNow.AddDays(30);
        }

        // Создание записи URL
        var url = new Url
        {
            OriginalUrl = request.OriginalUrl,
            ShortCode = shortCode,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            CustomCode = !string.IsNullOrEmpty(request.CustomCode) ? request.CustomCode : null,
            IsActive = true
        };

        await urlRepository.AddAsync(url);
        await urlRepository.SaveChangesAsync();

        return new UrlResponse
        {
            Id = url.Id,
            OriginalUrl = url.OriginalUrl,
            ShortCode = url.ShortCode,
            ShortUrl = GetFullShortUrl(shortCode),
            UserId = url.UserId,
            CreatedAt = url.CreatedAt,
            ExpiresAt = url.ExpiresAt,
            IsActive = url.IsActive,
            ClickCount = 0
        };
    }

    public async Task<UrlResponse> GetUrlByShortCodeAsync(string shortCode)
    {
        var url = await urlRepository.GetByShortCodeAsync(shortCode);
        if (url == null)
            return null;

        int clickCount = await clickDataRepository.GetClickCountByUrlIdAsync(url.Id);

        return new UrlResponse
        {
            Id = url.Id,
            OriginalUrl = url.OriginalUrl,
            ShortCode = url.ShortCode,
            ShortUrl = GetFullShortUrl(shortCode),
            UserId = url.UserId,
            CreatedAt = url.CreatedAt,
            ExpiresAt = url.ExpiresAt,
            IsActive = url.IsActive,
            ClickCount = clickCount
        };
    }

    public async Task<IEnumerable<UrlResponse>> GetUrlsByUserIdAsync(int userId)
    {
        var urls = await urlRepository.GetByUserIdAsync(userId);
        var responses = new List<UrlResponse>();

        foreach (var url in urls)
        {
            int clickCount = await clickDataRepository.GetClickCountByUrlIdAsync(url.Id);

            responses.Add(new UrlResponse
            {
                Id = url.Id,
                OriginalUrl = url.OriginalUrl,
                ShortCode = url.ShortCode,
                ShortUrl = GetFullShortUrl(url.ShortCode),
                UserId = url.UserId,
                CreatedAt = url.CreatedAt,
                ExpiresAt = url.ExpiresAt,
                IsActive = url.IsActive,
                ClickCount = clickCount
            });
        }

        return responses;
    }

    public async Task<bool> DeleteUrlAsync(string shortCode, int userId)
    {
        var url = await urlRepository.GetByShortCodeAsync(shortCode);
        if (url == null || url.UserId != userId)
            return false;

        await urlRepository.DeleteAsync(url);
        await urlRepository.SaveChangesAsync();
        return true;
    }

    public async Task<string> RedirectAndTrackAsync(string shortCode, string ipAddress, string userAgent,
        string referer)
    {
        var url = await urlRepository.GetByShortCodeAsync(shortCode);
        if (url == null || !url.IsActive)
            return null;

        // Проверка срока действия
        if (url.ExpiresAt.HasValue && url.ExpiresAt.Value < DateTime.UtcNow)
        {
            url.IsActive = false;
            await urlRepository.UpdateAsync(url);
            await urlRepository.SaveChangesAsync();
            return null;
        }

        // Определение устройства и браузера из User-Agent
        string deviceType = "Unknown";
        string browser = "Unknown";

        // Простой парсинг User-Agent для определения устройства и браузера
        if (!string.IsNullOrEmpty(userAgent))
        {
            // Определение устройства
            if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
                deviceType = "Mobile";
            else if (userAgent.Contains("iPad") || userAgent.Contains("Tablet"))
                deviceType = "Tablet";
            else
                deviceType = "Desktop";

            // Определение браузера
            if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg"))
                browser = "Chrome";
            else if (userAgent.Contains("Firefox"))
                browser = "Firefox";
            else if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
                browser = "Safari";
            else if (userAgent.Contains("Edg"))
                browser = "Edge";
            else if (userAgent.Contains("MSIE") || userAgent.Contains("Trident"))
                browser = "Internet Explorer";
            else
                browser = "Other";
        }

        // Определение страны и города по IP (для MVP используем упрощенную логику)
        string country = "Unknown";
        string city = "Unknown";

        // Добавляем информацию о клике
        var clickData = new ClickData
        {
            UrlId = url.Id,
            ClickedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            Country = country,
            City = city,
            DeviceType = deviceType,
            Browser = browser,
            ReferrerUrl = referer
        };

        await clickDataRepository.AddAsync(clickData);
        await clickDataRepository.SaveChangesAsync();

        return url.OriginalUrl;
    }

    public async Task<bool> IsUserAllowedToCreateMoreUrlsAsync(int? userId)
    {
        // Лимиты URL для разных типов пользователей
        int freeAnonymousLimit = 10;
        int freeRegisteredLimit = 50;

        if (!userId.HasValue)
        {
            // Для анонимных пользователей мы не можем отследить лимит, но в реальном приложении
            // можно использовать сессию или cookie для этого
            return true;
        }

        // Если пользователь премиум - нет ограничений
        if (await IsUserPremiumAsync(userId.Value))
        {
            return true;
        }

        // Иначе проверяем лимит
        int userUrlCount = await urlRepository.GetUrlCountByUserIdAsync(userId);
        return userUrlCount < freeRegisteredLimit;
    }

    // Вспомогательные методы
    private async Task<string> GenerateUniqueShortCodeAsync()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int codeLength = 6;
        bool isUnique = false;
        string shortCode = null;

        while (!isUnique)
        {
            StringBuilder sb = new StringBuilder(codeLength);
            for (int i = 0; i < codeLength; i++)
            {
                sb.Append(chars[_random.Next(chars.Length)]);
            }

            shortCode = sb.ToString();

            isUnique = !await urlRepository.ShortCodeExistsAsync(shortCode);
        }

        return shortCode;
    }

    private string GetFullShortUrl(string shortCode)
    {
        string baseUrl = configuration["BaseUrl"] ?? "http://localhost:5000";
        return $"{baseUrl}/{shortCode}";
    }

    private async Task<bool> IsUserPremiumAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user is { IsPremium: true };
    }
}