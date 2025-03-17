using UrlShortenerPro.Core.Dtos;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUrlRepository
{
    Task<UrlDto?> GetByIdAsync(int id);
    Task<UrlDto?> GetByShortCodeAsync(string shortCode);
    Task<List<UrlDto>> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10);
    Task<UrlDto> CreateAsync(UrlDto url);
    Task<bool> UpdateAsync(UrlDto url);
    Task<bool> DeleteAsync(int id);
    Task<bool> IncrementClickCountAsync(int id);
    Task<int> GetTotalUrlCountAsync();
    Task<int> GetActiveUrlCountAsync();
    Task<int> GetUrlCountByUserIdAsync(int userId);
    Task<bool> ShortCodeExistsAsync(string shortCode);
} 