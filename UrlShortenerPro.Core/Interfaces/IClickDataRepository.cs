using UrlShortenerPro.Core.Dtos;

namespace UrlShortenerPro.Core.Interfaces;

public interface IClickDataRepository
{
    Task<ClickDataDto> CreateAsync(ClickDataDto clickData);
    Task<List<ClickDataDto>> GetByUrlIdAsync(int urlId, int page = 1, int pageSize = 20);
    Task<List<ClickDataDto>> GetByUrlIdAndDateRangeAsync(int urlId, DateTime startDate, DateTime endDate);
    Task<int> GetTotalClicksAsync();
    Task<int> GetTotalClicksForUrlAsync(int urlId);
    Task<int> GetTotalClicksForUserAsync(int userId);
} 