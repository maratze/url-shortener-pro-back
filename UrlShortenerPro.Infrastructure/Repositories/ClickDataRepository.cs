using Microsoft.EntityFrameworkCore;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Interfaces;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class ClickDataRepository(AppDbContext context) : Repository<ClickData>(context), IClickDataRepository
{
    public async Task<IEnumerable<ClickData>> GetByUrlIdAsync(int urlId)
    {
        return await DbSet
            .Where(c => c.UrlId == urlId)
            .OrderByDescending(c => c.ClickedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClickData>> GetByUrlIdAndDateRangeAsync(int urlId, DateTime startDate,
        DateTime endDate)
    {
        return await DbSet
            .Where(c => c.UrlId == urlId && c.ClickedAt >= startDate && c.ClickedAt <= endDate)
            .OrderByDescending(c => c.ClickedAt)
            .ToListAsync();
    }

    public async Task<int> GetClickCountByUrlIdAsync(int urlId)
    {
        return await DbSet.CountAsync(c => c.UrlId == urlId);
    }

    public async Task<Dictionary<string, int>> GetDeviceStatsByUrlIdAsync(int urlId)
    {
        return await DbSet
            .Where(c => c.UrlId == urlId)
            .GroupBy(c => c.DeviceType)
            .Select(g => new { DeviceType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeviceType, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetBrowserStatsByUrlIdAsync(int urlId)
    {
        return await DbSet
            .Where(c => c.UrlId == urlId)
            .GroupBy(c => c.Browser)
            .Select(g => new { Browser = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Browser, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetLocationStatsByUrlIdAsync(int urlId)
    {
        return await DbSet
            .Where(c => c.UrlId == urlId)
            .GroupBy(c => c.Country)
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Country, x => x.Count);
    }

    // Новые методы для PostgreSQL версии
    public async Task<Dictionary<string, int>> GetReferrerStatsByUrlIdAsync(int urlId)
    {
        var referrerStats = await DbSet
            .Where(c => c.UrlId == urlId)
            .GroupBy(c => string.IsNullOrEmpty(c.ReferrerUrl) ? "Direct" : ExtractDomain(c.ReferrerUrl))
            .Select(g => new { Referrer = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Referrer, x => x.Count);

        return referrerStats;
    }

    public async Task<Dictionary<int, int>> GetHourlyStatsByUrlIdAsync(int urlId)
    {
        var clicks = await DbSet
            .Where(c => c.UrlId == urlId)
            .ToListAsync();
                
        var hourlyStats = clicks
            .GroupBy(c => c.ClickedAt.Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        return hourlyStats;
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