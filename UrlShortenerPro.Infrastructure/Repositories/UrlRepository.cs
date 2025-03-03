using Microsoft.EntityFrameworkCore;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Interfaces;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class UrlRepository(AppDbContext context) : Repository<Url>(context), IUrlRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Url?> GetByShortCodeAsync(string shortCode)
    {
        return await _context.Urls
            .Include(u => u.Clicks)
            .FirstOrDefaultAsync(u => u.ShortCode == shortCode);
    }

    public async Task<IEnumerable<Url>> GetByUserIdAsync(int userId)
    {
        return await _context.Urls
            .Where(u => u.UserId == userId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ShortCodeExistsAsync(string shortCode)
    {
        return await _context.Urls.AnyAsync(u => u.ShortCode == shortCode);
    }

    public async Task<int> GetUrlCountByUserIdAsync(int? userId)
    {
        if (userId.HasValue)
        {
            return await _context.Urls.CountAsync(u => u.UserId == userId);
        }

        return 0;
    }
}