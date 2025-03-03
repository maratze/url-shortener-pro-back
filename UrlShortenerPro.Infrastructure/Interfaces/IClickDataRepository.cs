using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Interfaces;

public interface IClickDataRepository : IRepository<ClickData>
{
    Task<IEnumerable<ClickData>> GetByUrlIdAsync(int urlId);
    Task<IEnumerable<ClickData>> GetByUrlIdAndDateRangeAsync(int urlId, DateTime startDate, DateTime endDate);
    Task<int> GetClickCountByUrlIdAsync(int urlId);
    Task<Dictionary<string, int>> GetDeviceStatsByUrlIdAsync(int urlId);
    Task<Dictionary<string, int>> GetBrowserStatsByUrlIdAsync(int urlId);
    Task<Dictionary<string, int>> GetLocationStatsByUrlIdAsync(int urlId);
    Task<Dictionary<string, int>> GetReferrerStatsByUrlIdAsync(int urlId);
    Task<Dictionary<int, int>> GetHourlyStatsByUrlIdAsync(int urlId);
}