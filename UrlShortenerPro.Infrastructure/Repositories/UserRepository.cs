using Microsoft.EntityFrameworkCore;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Interfaces;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class UserRepository(AppDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await DbSet.AnyAsync(u => u.Email == email);
    }
        
    // Получение пользователей, которые не заходили длительное время
    public async Task<IEnumerable<User>> GetInactiveUsersAsync(int daysThreshold)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-daysThreshold);
            
        return await DbSet
            .Where(u => u.LastLoginAt < thresholdDate || u.LastLoginAt == null)
            .OrderBy(u => u.LastLoginAt)
            .ToListAsync();
    }
        
    // Метод для получения премиум пользователей
    public async Task<IEnumerable<User>> GetPremiumUsersAsync()
    {
        return await DbSet
            .Where(u => u.IsPremium)
            .ToListAsync();
    }
        
    // Метод для обновления статуса премиум пользователя
    public async Task UpdatePremiumStatusAsync(int userId, bool isPremium)
    {
        var user = await DbSet.FindAsync(userId);
        if (user != null)
        {
            user.IsPremium = isPremium;
            await _dbContext.SaveChangesAsync();
        }
    }
}