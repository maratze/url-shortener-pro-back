using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Core.Interfaces;

public interface IAnalyticsService
{
    Task<ClickStats> GetClickStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null);
    Task<DeviceStats> GetDeviceStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null);
    Task<LocationStats> GetLocationStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ReferrerStats> GetReferrerStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> UserCanAccessUrlAnalyticsAsync(int urlId, int userId);
    Task<bool> UserCanAccessPremiumAnalyticsAsync(int userId);
}