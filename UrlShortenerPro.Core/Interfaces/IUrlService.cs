using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUrlService
{
    Task<UrlResponse> CreateShortUrlAsync(UrlCreationRequest request);
    Task<UrlResponse> GetUrlByShortCodeAsync(string shortCode);
    Task<IEnumerable<UrlResponse>> GetUrlsByUserIdAsync(int userId);
    Task<bool> DeleteUrlAsync(string shortCode, int userId);
    Task<string> RedirectAndTrackAsync(string shortCode, string ipAddress, string userAgent, string referer);
}