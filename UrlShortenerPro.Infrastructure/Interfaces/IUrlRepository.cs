using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Interfaces;

public interface IUrlRepository : IRepository<Url>
{
    Task<Url?> GetByShortCodeAsync(string shortCode);
    Task<IEnumerable<Url>> GetByUserIdAsync(int userId);
    Task<bool> ShortCodeExistsAsync(string shortCode);
    Task<int> GetUrlCountByUserIdAsync(int? userId);
}