using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Interfaces;

public interface IClickDataRepository : IRepository<ClickData>
{
    Task<IEnumerable<ClickData>> GetByUrlIdAsync(int urlId);
    Task<IEnumerable<ClickData>> GetByUrlIdAndDateRangeAsync(int urlId, DateTime startDate, DateTime endDate);
    Task<int> GetClickCountByUrlIdAsync(int urlId);
}