using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;
using UrlShortenerPro.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortenerPro.Core.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IClickDataRepository _clickDataRepository;
    private readonly IUrlRepository _urlRepository;
    private readonly IUserRepository _userRepository;

    public AnalyticsService(
        IClickDataRepository clickDataRepository,
        IUrlRepository urlRepository,
        IUserRepository userRepository)
    {
        _clickDataRepository = clickDataRepository;
        _urlRepository = urlRepository;
        _userRepository = userRepository;
    }

    public async Task<object> GetUrlAnalyticsAsync(int urlId, int userId)
    {
        // Verify that the user can access the URL
        if (!await UserCanAccessUrlAnalyticsAsync(urlId, userId))
        {
            throw new UnauthorizedAccessException();
        }

        var url = await _urlRepository.GetByIdAsync(urlId);
        if (url == null)
        {
            return null;
        }

        var totalClicks = await _clickDataRepository.GetTotalClicksForUrlAsync(urlId);
        var clickStats = await GetClickStatsAsync(urlId);

        return new
        {
            UrlId = url.Id,
            ShortCode = url.ShortCode,
            OriginalUrl = url.OriginalUrl,
            TotalClicks = totalClicks,
            ClicksByDate = clickStats.ClicksByDate
        };
    }

    public async Task<object> GetUserAnalyticsAsync(int userId)
    {
        var totalUrls = await _urlRepository.GetUrlCountByUserIdAsync(userId);
        var totalClicks = await _clickDataRepository.GetTotalClicksForUserAsync(userId);

        var urls = await _urlRepository.GetByUserIdAsync(userId);
        var topUrls = urls
            .OrderByDescending(u => u.ClickCount)
            .Take(5)
            .Select(u => new
            {
                Id = u.Id,
                ShortCode = u.ShortCode,
                OriginalUrl = u.OriginalUrl,
                ClickCount = u.ClickCount
            })
            .ToList();

        return new
        {
            TotalUrls = totalUrls,
            TotalClicks = totalClicks,
            TopUrls = topUrls
        };
    }

    public async Task<IEnumerable<ClickDataDto>> GetUrlClicksAsync(int urlId, int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Verify that the user can access the URL
        if (!await UserCanAccessUrlAnalyticsAsync(urlId, userId))
        {
            throw new UnauthorizedAccessException();
        }

        var now = DateTime.UtcNow;
        startDate ??= now.AddDays(-30);
        endDate ??= now;

        var clicks = await _clickDataRepository.GetByUrlIdAndDateRangeAsync(
            urlId, startDate.Value, endDate.Value);

        return clicks;
    }

    public async Task<ClickStats> GetClickStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        startDate ??= now.AddDays(-30);
        endDate ??= now;

        var clicks = await _clickDataRepository.GetByUrlIdAndDateRangeAsync(
            urlId, startDate.Value, endDate.Value);

        var stats = new ClickStats
        {
            TotalClicks = clicks.Count(),
            ClicksByDate = new Dictionary<DateTime, int>()
        };

        // Group by date
        var clicksByDate = clicks
            .GroupBy(c => c.ClickedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() });

        foreach (var item in clicksByDate)
        {
            stats.ClicksByDate[item.Date] = item.Count;
        }

        return stats;
    }

    public async Task<object> GetDeviceStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        startDate ??= now.AddDays(-30);
        endDate ??= now;

        var clicks = await _clickDataRepository.GetByUrlIdAndDateRangeAsync(
            urlId, startDate.Value, endDate.Value);

        var deviceTypes = clicks
            .GroupBy(c => c.DeviceType ?? "Unknown")
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Type, x => x.Count);

        var browsers = clicks
            .GroupBy(c => c.Browser ?? "Unknown")
            .Select(g => new { Browser = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Browser, x => x.Count);

        var operatingSystems = clicks
            .GroupBy(c => c.OperatingSystem ?? "Unknown")
            .Select(g => new { OS = g.Key, Count = g.Count() })
            .ToDictionary(x => x.OS, x => x.Count);

        return new
        {
            DeviceTypes = deviceTypes,
            Browsers = browsers,
            OperatingSystems = operatingSystems
        };
    }

    public async Task<object> GetLocationStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        startDate ??= now.AddDays(-30);
        endDate ??= now;

        var clicks = await _clickDataRepository.GetByUrlIdAndDateRangeAsync(
            urlId, startDate.Value, endDate.Value);

        var countries = clicks
            .GroupBy(c => c.Country ?? "Unknown")
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Country, x => x.Count);

        var cities = clicks
            .GroupBy(c => c.City ?? "Unknown")
            .Select(g => new { City = g.Key, Count = g.Count() })
            .ToDictionary(x => x.City, x => x.Count);

        return new
        {
            Countries = countries,
            Cities = cities
        };
    }

    public async Task<object> GetReferrerStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        startDate ??= now.AddDays(-30);
        endDate ??= now;

        var clicks = await _clickDataRepository.GetByUrlIdAndDateRangeAsync(
            urlId, startDate.Value, endDate.Value);

        var referrers = clicks
            .GroupBy(c => string.IsNullOrEmpty(c.ReferrerUrl) ? "Direct" : ExtractDomain(c.ReferrerUrl))
            .Select(g => new { Referrer = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Referrer, x => x.Count);

        return new
        {
            Referrers = referrers
        };
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync(int userId, int days = 7)
    {
        // Получаем все ссылки пользователя
        var allUserUrls = await _urlRepository.GetByUserIdAsync(userId);
        
        // Текущая дата для расчета новых ссылок
        var currentDate = DateTime.UtcNow;
        var periodStartDate = currentDate.AddDays(-days);
        
        // Считаем общую статистику
        var totalLinks = allUserUrls.Count;
        var activeLinks = allUserUrls.Count(u => u.IsActive);
        var linksWithQrCodes = allUserUrls.Count(u => u.HasQrCode);
        
        // Считаем новые ссылки и ссылки с QR-кодами за указанный период
        var newLinks = allUserUrls.Count(u => u.CreatedAt >= periodStartDate);
        var newQrCodes = allUserUrls.Count(u => u.HasQrCode && u.CreatedAt >= periodStartDate);
        
        // Получаем общее количество кликов
        var totalClicks = await _clickDataRepository.GetTotalClicksForUserAsync(userId);
        
        // Расчет процента активных ссылок
        var activeLinksPercentage = totalLinks > 0 ? (double)activeLinks / totalLinks * 100 : 0;
        
        return new DashboardStatsResponse
        {
            TotalLinks = totalLinks,
            LinksWithQrCodes = linksWithQrCodes,
            ActiveLinks = activeLinks,
            ActiveLinksPercentage = Math.Round(activeLinksPercentage, 1),
            TotalClicks = totalClicks,
            NewLinks = newLinks,
            NewQrCodes = newQrCodes
        };
    }

    public async Task<bool> UserCanAccessUrlAnalyticsAsync(int urlId, int userId)
    {
        var url = await _urlRepository.GetByIdAsync(urlId);
        return url != null && url.UserId == userId;
    }

    public async Task<bool> UserCanAccessPremiumAnalyticsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null && user.IsPremium;
    }

    // Helper method to extract domain from URL
    private string ExtractDomain(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
                return "Unknown";

            if (!url.StartsWith("http"))
                url = "http://" + url;

            var uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return "Invalid URL";
        }
    }
}