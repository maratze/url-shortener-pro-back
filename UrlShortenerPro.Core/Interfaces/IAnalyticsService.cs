using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Core.Interfaces;

public interface IAnalyticsService
{
    Task<object> GetUrlAnalyticsAsync(int urlId, int userId);
    Task<object> GetUserAnalyticsAsync(int userId);
    Task<IEnumerable<ClickDataDto>> GetUrlClicksAsync(int urlId, int userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> UserCanAccessUrlAnalyticsAsync(int urlId, int userId);
    Task<bool> UserCanAccessPremiumAnalyticsAsync(int userId);
    Task<object> GetDeviceStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetLocationStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetReferrerStatsAsync(int urlId, DateTime? startDate = null, DateTime? endDate = null);
}