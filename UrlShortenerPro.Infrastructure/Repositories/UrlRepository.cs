using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class UrlRepository : IUrlRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UrlRepository> _logger;

    public UrlRepository(AppDbContext dbContext, ILogger<UrlRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UrlDto?> GetByIdAsync(int id)
    {
        try
        {
            var url = await _dbContext.Urls
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return url != null ? MapToDto(url) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving URL with ID {UrlId}", id);
            return null;
        }
    }

    public async Task<UrlDto?> GetByShortCodeAsync(string shortCode)
    {
        try
        {
            var url = await _dbContext.Urls
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            return url != null ? MapToDto(url) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving URL with short code {ShortCode}", shortCode);
            return null;
        }
    }

    public async Task<List<UrlDto>> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var skip = (page - 1) * pageSize;

            var urls = await _dbContext.Urls
                .AsNoTracking()
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return urls.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving URLs for user {UserId}", userId);
            return new List<UrlDto>();
        }
    }

    public async Task<UrlDto> CreateAsync(UrlDto urlDto)
    {
        try
        {
            var url = MapToEntity(urlDto);
            _dbContext.Urls.Add(url);
            await _dbContext.SaveChangesAsync();
            return MapToDto(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating URL {ShortCode}", urlDto.ShortCode);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(UrlDto urlDto)
    {
        try
        {
            var url = await _dbContext.Urls.FindAsync(urlDto.Id);
            if (url == null) return false;
            
            url.OriginalUrl = urlDto.OriginalUrl;
            url.ShortCode = urlDto.ShortCode;
            url.ExpiresAt = urlDto.ExpiresAt;
            url.IsActive = urlDto.IsActive;
            url.ClickCount = urlDto.ClickCount;
            url.HasQrCode = urlDto.HasQrCode;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating URL with ID {UrlId}", urlDto.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var url = await _dbContext.Urls.FindAsync(id);
            if (url == null) return false;
            
            _dbContext.Urls.Remove(url);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting URL with ID {UrlId}", id);
            return false;
        }
    }

    public async Task<bool> IncrementClickCountAsync(int id)
    {
        try
        {
            var url = await _dbContext.Urls.FindAsync(id);
            if (url == null) return false;
            
            url.ClickCount++;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing click count for URL with ID {UrlId}", id);
            return false;
        }
    }

    public async Task<int> GetTotalUrlCountAsync()
    {
        try
        {
            return await _dbContext.Urls.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total URL count");
            return 0;
        }
    }

    public async Task<int> GetActiveUrlCountAsync()
    {
        try
        {
            return await _dbContext.Urls.CountAsync(u => u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active URL count");
            return 0;
        }
    }

    public async Task<int> GetUrlCountByUserIdAsync(int userId)
    {
        try
        {
            return await _dbContext.Urls.CountAsync(u => u.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting URL count for user {UserId}", userId);
            return 0;
        }
    }

    public async Task<bool> ShortCodeExistsAsync(string shortCode)
    {
        try
        {
            return await _dbContext.Urls.AnyAsync(u => u.ShortCode == shortCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if short code {ShortCode} exists", shortCode);
            return false;
        }
    }

    // Метод для получения активных URL с истекающим сроком действия
    // Может использоваться для системы уведомлений о скором истечении ссылок
    public async Task<IEnumerable<Url>> GetExpiringUrlsAsync(int daysThreshold)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
        
        return await _dbContext.Urls
            .Where(u => u.IsActive && 
                        u.ExpiresAt.HasValue && 
                        u.ExpiresAt <= thresholdDate)
            .OrderBy(u => u.ExpiresAt)
            .ToListAsync();
    }
    
    // Метод для очистки истекших URL
    // Может использоваться для фоновой задачи деактивации
    public async Task DeactivateExpiredUrlsAsync()
    {
        var now = DateTime.UtcNow;
        var expiredUrls = await _dbContext.Urls
            .Where(u => u.IsActive && 
                        u.ExpiresAt.HasValue && 
                        u.ExpiresAt < now)
            .ToListAsync();
            
        foreach (var url in expiredUrls)
        {
            url.IsActive = false;
        }
        
        await _dbContext.SaveChangesAsync();
    }

    // Helper methods to map between entity and DTO
    private UrlDto MapToDto(Url url)
    {
        return new UrlDto
        {
            Id = url.Id,
            OriginalUrl = url.OriginalUrl,
            ShortCode = url.ShortCode,
            UserId = url.UserId,
            CreatedAt = url.CreatedAt,
            ExpiresAt = url.ExpiresAt,
            IsActive = url.IsActive,
            ClickCount = url.ClickCount,
            HasQrCode = url.HasQrCode
        };
    }

    private Url MapToEntity(UrlDto dto)
    {
        return new Url
        {
            Id = dto.Id,
            OriginalUrl = dto.OriginalUrl,
            ShortCode = dto.ShortCode,
            UserId = dto.UserId,
            CreatedAt = dto.CreatedAt,
            ExpiresAt = dto.ExpiresAt,
            IsActive = dto.IsActive,
            ClickCount = dto.ClickCount,
            HasQrCode = dto.HasQrCode
        };
    }
}