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
    IConfiguration configuration)
    : IUrlService
{
    private readonly Random _random = new();
    private readonly string _baseUrl = configuration["BaseUrl"] ?? "https://localhost:7095";

    public async Task<UrlResponse> CreateShortUrlAsync(UrlCreationRequest request)
    {
        // Генерация короткого кода
        string shortCode;

        if (!string.IsNullOrEmpty(request.CustomCode))
        {
            bool exists = await urlRepository.ShortCodeExistsAsync(request.CustomCode);
            if (exists)
            {
                throw new InvalidOperationException("Этот кастомный код уже занят");
            }

            shortCode = request.CustomCode;
        }
        else
        {
            shortCode = await GenerateUniqueShortCodeAsync();
        }

        // Расчет срока истечения (по умолчанию 30 дней)
        DateTime? expiresAt = DateTime.UtcNow.AddDays(request.ExpiresInDays ?? 30);

        // Создание записи URL
        var url = new Url
        {
            OriginalUrl = request.OriginalUrl,
            ShortCode = shortCode,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = true
        };

        await urlRepository.AddAsync(url);
        await urlRepository.SaveChangesAsync();

        return new UrlResponse
        {
            Id = url.Id,
            OriginalUrl = url.OriginalUrl,
            ShortCode = url.ShortCode,
            ShortUrl = $"{_baseUrl}/{shortCode}",
            UserId = url.UserId,
            CreatedAt = url.CreatedAt,
            ExpiresAt = url.ExpiresAt,
            IsActive = url.IsActive,
            ClickCount = 0
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
                ShortUrl = $"{_baseUrl}/{url.ShortCode}",
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

        // В MVP версии просто деактивируем URL вместо удаления
        url.IsActive = false;
        await urlRepository.UpdateAsync(url);
        await urlRepository.SaveChangesAsync();
        return true;
    }

    public async Task<UrlResponse> GetUrlByShortCodeAsync(string shortCode)
    {
        var url = await urlRepository.GetByShortCodeAsync(shortCode);

        if (url == null || !url.IsActive || (url.ExpiresAt.HasValue && url.ExpiresAt < DateTime.UtcNow))
        {
            return null;
        }

        int clickCount = await clickDataRepository.GetClickCountByUrlIdAsync(url.Id);

        return new UrlResponse
        {
            Id = url.Id,
            OriginalUrl = url.OriginalUrl,
            ShortCode = url.ShortCode,
            ShortUrl = $"{_baseUrl}/{url.ShortCode}",
            UserId = url.UserId,
            CreatedAt = url.CreatedAt,
            ExpiresAt = url.ExpiresAt,
            IsActive = url.IsActive,
            ClickCount = clickCount
        };
    }

    public async Task<string> RedirectAndTrackAsync(string shortCode, string ipAddress, string userAgent,
        string referer)
    {
        var url = await urlRepository.GetByShortCodeAsync(shortCode);

        // Проверяем, существует и активна ли ссылка
        if (url == null || !url.IsActive || (url.ExpiresAt.HasValue && url.ExpiresAt < DateTime.UtcNow))
        {
            return null;
        }

        // Упрощенная версия для MVP - записываем только базовую информацию о клике
        var clickData = new ClickData
        {
            UrlId = url.Id,
            ClickedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            Browser = GetSimpleBrowser(userAgent),
            DeviceType = GetSimpleDeviceType(userAgent),
            ReferrerUrl = referer
        };

        await clickDataRepository.AddAsync(clickData);
        await clickDataRepository.SaveChangesAsync();

        return url.OriginalUrl;
    }

    // Простые вспомогательные методы для MVP
    private string GetSimpleBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        if (userAgent.Contains("Chrome")) return "Chrome";
        if (userAgent.Contains("Firefox")) return "Firefox";
        if (userAgent.Contains("Safari")) return "Safari";
        if (userAgent.Contains("Edge")) return "Edge";
        return "Other";
    }

    private string GetSimpleDeviceType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        if (userAgent.Contains("Mobile")) return "Mobile";
        if (userAgent.Contains("Tablet")) return "Tablet";
        return "Desktop";
    }

    // Метод для генерации уникального кода
    private async Task<string> GenerateUniqueShortCodeAsync()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int codeLength = 6;
        bool isUnique = false;
        string shortCode = null;

        while (!isUnique)
        {
            var sb = new StringBuilder(codeLength);
            for (int i = 0; i < codeLength; i++)
            {
                sb.Append(chars[_random.Next(chars.Length)]);
            }

            shortCode = sb.ToString();
            isUnique = !await urlRepository.ShortCodeExistsAsync(shortCode);
        }

        return shortCode;
    }
}