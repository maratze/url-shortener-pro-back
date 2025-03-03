using Microsoft.EntityFrameworkCore;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Interfaces;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

 public class UrlRepository(AppDbContext dbContext) : Repository<Url>(dbContext), IUrlRepository
 {
        public async Task<Url?> GetByShortCodeAsync(string shortCode)
        {
            return await DbSet
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);
        }

        public async Task<IEnumerable<Url>> GetByUserIdAsync(int userId)
        {
            return await DbSet
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ShortCodeExistsAsync(string shortCode)
        {
            return await DbSet.AnyAsync(u => u.ShortCode == shortCode);
        }

        public async Task<int> GetUrlCountByUserIdAsync(int? userId)
        {
            if (!userId.HasValue)
                return 0;
                
            return await DbSet.CountAsync(u => u.UserId == userId);
        }

        // Метод для получения активных URL с истекающим сроком действия
        // Может использоваться для системы уведомлений о скором истечении ссылок
        public async Task<IEnumerable<Url>> GetExpiringUrlsAsync(int daysThreshold)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
            
            return await DbSet
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
            var expiredUrls = await DbSet
                .Where(u => u.IsActive && 
                            u.ExpiresAt.HasValue && 
                            u.ExpiresAt < now)
                .ToListAsync();
                
            foreach (var url in expiredUrls)
            {
                url.IsActive = false;
            }
            
            await dbContext.SaveChangesAsync();
        }
    }