using UrlShortenerPro.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortenerPro.Core.Dtos;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUrlService
{
    Task<UrlResponse> CreateShortUrlAsync(UrlCreationRequest request);
    Task<UrlResponse?> GetUrlByShortCodeAsync(string shortCode);
    Task<List<UrlResponse>> GetUrlsByUserIdAsync(int userId, int page = 1, int pageSize = 10);
    Task<bool> TrackClickAsync(string shortCode, ClickTrackingData trackingData);
    Task<UrlDto> GetByIdAsync(int id);
    Task<UrlDto> GetByShortCodeAsync(string shortCode);
    Task<IEnumerable<UrlDto>> GetByUserIdAsync(int userId);
    Task<UrlDto> CreateAsync(UrlDto urlDto);
    Task<UrlDto> UpdateAsync(UrlDto urlDto);
    Task<bool> DeleteAsync(int id);
    Task<string> GetOriginalUrlAndTrackClickAsync(string shortCode, string ipAddress, string userAgent, string referer);
    Task<bool> ShortCodeExistsAsync(string shortCode);
    Task<int> GetTotalUrlCountAsync();
    Task<int> GetActiveUrlCountAsync();
    Task<int> GetUrlCountByUserIdAsync(int userId);
    Task<bool> DeleteUrlAsync(string shortCode, int userId);
}