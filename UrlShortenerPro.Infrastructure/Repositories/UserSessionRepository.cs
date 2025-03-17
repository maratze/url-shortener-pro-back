using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserSessionRepository> _logger;

    public UserSessionRepository(AppDbContext dbContext, ILogger<UserSessionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserSessionDto> CreateAsync(UserSessionDto sessionDto)
    {
        try
        {
            var session = MapToEntity(sessionDto);
            _dbContext.UserSessions.Add(session);
            await _dbContext.SaveChangesAsync();
            return MapToDto(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user session for user ID {UserId}", sessionDto.UserId);
            throw;
        }
    }

    public async Task<UserSessionDto?> GetByIdAsync(int id)
    {
        try
        {
            var session = await _dbContext.UserSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            return session != null ? MapToDto(session) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session with ID {SessionId}", id);
            return null;
        }
    }

    public async Task<UserSessionDto?> GetByTokenAsync(string token)
    {
        try
        {
            var session = await _dbContext.UserSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Token == token);

            return session != null ? MapToDto(session) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session by token");
            return null;
        }
    }

    public async Task<IEnumerable<UserSessionDto>> GetByUserIdAsync(int userId)
    {
        try
        {
            var sessions = await _dbContext.UserSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.LastActivityAt)
                .ToListAsync();

            return sessions.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions for user ID {UserId}", userId);
            return Enumerable.Empty<UserSessionDto>();
        }
    }

    public async Task<bool> UpdateAsync(UserSessionDto sessionDto)
    {
        try
        {
            var session = await _dbContext.UserSessions.FindAsync(sessionDto.Id);
            if (session == null) return false;

            session.LastActivityAt = sessionDto.LastActivityAt;
            session.IsActive = sessionDto.IsActive;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session with ID {SessionId}", sessionDto.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var session = await _dbContext.UserSessions.FindAsync(id);
            if (session == null) return false;

            _dbContext.UserSessions.Remove(session);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session with ID {SessionId}", id);
            return false;
        }
    }

    public async Task<bool> DeleteAllExceptAsync(int userId, int sessionIdToKeep)
    {
        try
        {
            var sessionsToDelete = await _dbContext.UserSessions
                .Where(s => s.UserId == userId && s.Id != sessionIdToKeep)
                .ToListAsync();

            _dbContext.UserSessions.RemoveRange(sessionsToDelete);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sessions for user ID {UserId}", userId);
            return false;
        }
    }

    private UserSessionDto MapToDto(UserSession session)
    {
        return new UserSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            DeviceInfo = session.DeviceInfo,
            IpAddress = session.IpAddress,
            Location = session.Location,
            CreatedAt = session.CreatedAt,
            LastActivityAt = session.LastActivityAt,
            Token = session.Token,
            IsActive = session.IsActive
        };
    }

    private UserSession MapToEntity(UserSessionDto sessionDto)
    {
        return new UserSession
        {
            Id = sessionDto.Id,
            UserId = sessionDto.UserId,
            DeviceInfo = sessionDto.DeviceInfo,
            IpAddress = sessionDto.IpAddress,
            Location = sessionDto.Location,
            CreatedAt = sessionDto.CreatedAt,
            LastActivityAt = sessionDto.LastActivityAt,
            Token = sessionDto.Token,
            IsActive = sessionDto.IsActive
        };
    }
} 