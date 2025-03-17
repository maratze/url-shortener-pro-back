using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class ClickDataRepository : IClickDataRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ClickDataRepository> _logger;

    public ClickDataRepository(AppDbContext dbContext, ILogger<ClickDataRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ClickDataDto> CreateAsync(ClickDataDto clickDataDto)
    {
        try
        {
            var clickData = MapToEntity(clickDataDto);
            _dbContext.ClickData.Add(clickData);
            await _dbContext.SaveChangesAsync();
            return MapToDto(clickData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating click data for URL ID {UrlId}", clickDataDto.UrlId);
            throw;
        }
    }

    public async Task<List<ClickDataDto>> GetByUrlIdAsync(int urlId, int page = 1, int pageSize = 20)
    {
        try
        {
            var skip = (page - 1) * pageSize;

            var clickData = await _dbContext.ClickData
                .AsNoTracking()
                .Where(c => c.UrlId == urlId)
                .OrderByDescending(c => c.ClickedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return clickData.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving click data for URL ID {UrlId}", urlId);
            return new List<ClickDataDto>();
        }
    }

    public async Task<List<ClickDataDto>> GetByUrlIdAndDateRangeAsync(int urlId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var clickData = await _dbContext.ClickData
                .AsNoTracking()
                .Where(c => c.UrlId == urlId && 
                            c.ClickedAt >= startDate && 
                            c.ClickedAt <= endDate)
                .OrderByDescending(c => c.ClickedAt)
                .ToListAsync();

            return clickData.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving click data for URL ID {UrlId} in date range", urlId);
            return new List<ClickDataDto>();
        }
    }

    public async Task<int> GetTotalClicksAsync()
    {
        try
        {
            return await _dbContext.ClickData.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total clicks count");
            return 0;
        }
    }

    public async Task<int> GetTotalClicksForUrlAsync(int urlId)
    {
        try
        {
            return await _dbContext.ClickData.CountAsync(c => c.UrlId == urlId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting click count for URL ID {UrlId}", urlId);
            return 0;
        }
    }

    public async Task<int> GetTotalClicksForUserAsync(int userId)
    {
        try
        {
            return await _dbContext.ClickData
                .Join(_dbContext.Urls,
                    click => click.UrlId,
                    url => url.Id,
                    (click, url) => new { click, url })
                .Where(x => x.url.UserId == userId)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total clicks for user {UserId}", userId);
            return 0;
        }
    }

    // Helper methods to map between entity and DTO
    private ClickDataDto MapToDto(ClickData clickData)
    {
        return new ClickDataDto
        {
            Id = clickData.Id,
            UrlId = clickData.UrlId,
            IpAddress = clickData.IpAddress,
            UserAgent = clickData.UserAgent,
            ReferrerUrl = clickData.ReferrerUrl,
            DeviceType = clickData.DeviceType,
            Browser = clickData.Browser,
            OperatingSystem = clickData.OperatingSystem,
            Country = clickData.Country,
            City = clickData.City,
            ClickedAt = clickData.ClickedAt
        };
    }

    private ClickData MapToEntity(ClickDataDto clickDataDto)
    {
        return new ClickData
        {
            Id = clickDataDto.Id,
            UrlId = clickDataDto.UrlId,
            IpAddress = clickDataDto.IpAddress,
            UserAgent = clickDataDto.UserAgent,
            ReferrerUrl = clickDataDto.ReferrerUrl,
            DeviceType = clickDataDto.DeviceType,
            Browser = clickDataDto.Browser,
            OperatingSystem = clickDataDto.OperatingSystem,
            Country = clickDataDto.Country,
            City = clickDataDto.City,
            ClickedAt = clickDataDto.ClickedAt
        };
    }
}