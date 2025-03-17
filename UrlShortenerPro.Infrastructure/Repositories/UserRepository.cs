using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext dbContext, ILogger<UserRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        try
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return user != null ? MapToDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return null;
        }
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        try
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            return user != null ? MapToDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with email {Email}", email);
            return null;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            return await _dbContext.Users.AnyAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if email {Email} exists", email);
            return false;
        }
    }

    public async Task<UserDto> CreateAsync(UserDto userDto)
    {
        try
        {
            var user = MapToEntity(userDto);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email {Email}", userDto.Email);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(UserDto userDto)
    {
        try
        {
            var user = await _dbContext.Users.FindAsync(userDto.Id);
            if (user == null) return false;
            
            user.Email = userDto.Email;
            user.PasswordHash = userDto.PasswordHash;
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.IsPremium = userDto.IsPremium;
            user.LastLoginAt = userDto.LastLoginAt;
            user.Role = userDto.Role;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", userDto.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null) return false;
            
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return false;
        }
    }

    public async Task<int> GetTotalUserCountAsync()
    {
        try
        {
            return await _dbContext.Users.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total user count");
            return 0;
        }
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        try
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            return await _dbContext.Users.CountAsync(u => u.LastLoginAt >= thirtyDaysAgo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active user count");
            return 0;
        }
    }

    // Helper methods to map between entity and DTO
    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsPremium = user.IsPremium,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Role = user.Role
        };
    }

    private User MapToEntity(UserDto userDto)
    {
        return new User
        {
            Id = userDto.Id,
            Email = userDto.Email,
            PasswordHash = userDto.PasswordHash,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            IsPremium = userDto.IsPremium,
            CreatedAt = userDto.CreatedAt,
            LastLoginAt = userDto.LastLoginAt,
            Role = userDto.Role
        };
    }
}