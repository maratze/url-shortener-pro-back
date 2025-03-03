using Microsoft.EntityFrameworkCore;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Interfaces;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class ClickDataRepository(AppDbContext context) : Repository<ClickData>(context), IClickDataRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<ClickData>> GetByUrlIdAsync(int urlId)
    {
        return await _context.ClickData
            .Where(c => c.UrlId == urlId)
            .OrderByDescending(c => c.ClickedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClickData>> GetByUrlIdAndDateRangeAsync(int urlId, DateTime startDate, DateTime endDate)
    {
        return await _context.ClickData
            .Where(c => c.UrlId == urlId && c.ClickedAt >= startDate && c.ClickedAt <= endDate)
            .OrderByDescending(c => c.ClickedAt)
            .ToListAsync();
    }
        
    public async Task<int> GetClickCountByUrlIdAsync(int urlId)
    {
        return await _context.ClickData.CountAsync(c => c.UrlId == urlId);
    }
}