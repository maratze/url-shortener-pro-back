using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;
using UrlShortenerPro.Infrastructure.Interfaces;

namespace UrlShortenerPro.Core.Services;

public class AnalyticsService(
    IClickDataRepository clickDataRepository,
    IUrlRepository urlRepository,
    IUserRepository userRepository)
    : IAnalyticsService
{
    public async Task<ClickStats> GetClickStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        startDate ??= now.AddDays(-30);
        endDate ??= now;

        var clicks = await clickDataRepository.GetByUrlIdAndDateRangeAsync(
            urlId, startDate.Value, endDate.Value);

        var stats = new ClickStats
        {
            TotalClicks = clicks.Count(),
            ClicksByDate = new Dictionary<DateTime, int>()
        };

        // Группировка по дате
        var clicksByDate = clicks
            .GroupBy(c => c.ClickedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() });

        foreach (var item in clicksByDate)
        {
            stats.ClicksByDate[item.Date] = item.Count;
        }

        return stats;
    }

    public async Task<DeviceStats> GetDeviceStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        startDate ??= now.AddDays(-30);
        endDate ??= now;

        var clicks = await clickDataRepository.GetByUrlIdAndDateRangeAsync(
            urlId, startDate.Value, endDate.Value);

        var stats = new DeviceStats
        {
            DeviceTypes = new Dictionary<string, int>(),
            Browsers = new Dictionary<string, int>()
        };

        var deviceTypes = clicks
            .GroupBy(c => c.DeviceType)
            .Select(g => new { Type = g.Key, Count = g.Count() });

        foreach (var item in deviceTypes)
        {
            stats.DeviceTypes[item.Type] = item.Count;
        }

        var browsers = clicks
            .GroupBy(c => c.Browser)
            .Select(g => new { Browser = g.Key, Count = g.Count() });

        foreach (var item in browsers)
        {
            stats.Browsers[item.Browser] = item.Count;
        }

        return stats;
    }

    public async Task<LocationStats> GetLocationStatsAsync(int urlId, DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        startDate ??= now.AddDays(-30);
        endDate ??= now;

        var clicks = await clickDataRepository.GetByUrlIdAndDateRangeAsync(
            urlId, startDate.Value, endDate.Value);

        var stats = new LocationStats
        {
            Countries = new Dictionary<string, int>(),
            Cities = new Dictionary<string, int>()
        };

        var countries = clicks
            .GroupBy(c => c.Country)
            .Select(g => new { Country = g.Key, Count = g.Count() });

        foreach (var item in countries)
        {
            stats.Countries[item.Country] = item.Count;
        }

        var cities = clicks
            .GroupBy(c => c.City)
            .Select(g => new { City = g.Key, Count = g.Count() });

        foreach (var item in cities)
        {
            stats.Cities[item.City] = item.Count;
        }

        return stats;
    }

    public async Task<ReferrerStats> GetReferrerStatsAsync(int urlId, DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        startDate ??= now.AddDays(-30);
        endDate ??= now;

        var clicks = await clickDataRepository.GetByUrlIdAndDateRangeAsync(
            urlId, startDate.Value, endDate.Value);

        var stats = new ReferrerStats
        {
            Referrers = new Dictionary<string, int>()
        };

        var referrers = clicks
            .GroupBy(c => string.IsNullOrEmpty(c.ReferrerUrl) ? "Direct" : ExtractDomain(c.ReferrerUrl))
            .Select(g => new { Referrer = g.Key, Count = g.Count() });

        foreach (var item in referrers)
        {
            stats.Referrers[item.Referrer] = item.Count;
        }

        return stats;
    }

    public async Task<bool> UserCanAccessUrlAnalyticsAsync(int urlId, int userId)
    {
        var url = await urlRepository.GetByIdAsync(urlId);
        return url != null && url.UserId == userId;
    }

    public async Task<bool> UserCanAccessPremiumAnalyticsAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user != null && user.IsPremium;
    }

    // Вспомогательный метод для извлечения домена из URL
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