using Microsoft.EntityFrameworkCore;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly AppDbContext _dbContext;
    
    public UserSessionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IEnumerable<UserSessionDto>> GetActiveSessionsByUserIdAsync(int userId)
    {
        var sessions = await _dbContext.UserSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync();
            
        return sessions.Select(MapToDto);
    }
    
    public async Task<UserSessionDto?> GetSessionByTokenAsync(int userId, string token)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Token == token && s.IsActive);
            
        return session != null ? MapToDto(session) : null;
    }
    
    public async Task<UserSessionDto?> GetSessionByIdAsync(int sessionId, int userId)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive);
            
        return session != null ? MapToDto(session) : null;
    }
    
    public async Task<bool> DeactivateSessionAsync(int sessionId, int userId)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive);
            
        if (session == null)
        {
            return false;
        }
        
        session.IsActive = false;
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<int> DeactivateAllSessionsExceptTokenAsync(int userId, string excludeToken)
    {
        var sessions = await _dbContext.UserSessions
            .Where(s => s.UserId == userId && s.Token != excludeToken && s.IsActive)
            .ToListAsync();
            
        foreach (var session in sessions)
        {
            session.IsActive = false;
        }
        
        if (sessions.Count > 0)
        {
            await _dbContext.SaveChangesAsync();
        }
        
        return sessions.Count;
    }
    
    public async Task<bool> UpdateSessionActivityAsync(int userId, string token)
    {
        try
        {
            var session = await _dbContext.UserSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Token == token && s.IsActive);

            if (session == null)
                return false;

            session.LastActivityAt = DateTime.UtcNow;
            _dbContext.UserSessions.Update(session);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public async Task AddSessionAsync(Core.Models.UserSession session)
    {
        var infraSession = new Models.UserSession
        {
            UserId = session.UserId,
            Token = session.Token,
            DeviceInfo = session.DeviceInfo,
            IpAddress = session.IpAddress,
            Location = session.Location,
            CreatedAt = session.CreatedAt,
            LastActivityAt = session.LastActivityAt,
            IsActive = session.IsActive
        };
        
        await _dbContext.UserSessions.AddAsync(infraSession);
        await _dbContext.SaveChangesAsync();
    }
    
    private static UserSessionDto MapToDto(Models.UserSession session)
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
} 