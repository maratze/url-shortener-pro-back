using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Core.Services;

public class UserSessionService : IUserSessionService
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly ILogger<UserSessionService> _logger;

    public UserSessionService(
        IUserSessionRepository sessionRepository,
        ILogger<UserSessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<UserSessionDto> CreateSessionAsync(int userId, string deviceInfo, string ipAddress, string location)
    {
        try
        {
            var token = GenerateSecureToken();
            
            var sessionDto = new UserSessionDto
            {
                UserId = userId,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                Location = location,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsActive = true
            };

            return await _sessionRepository.CreateAsync(sessionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<UserSessionDto>> GetSessionsByUserIdAsync(int userId)
    {
        try
        {
            return await _sessionRepository.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> RevokeSessionAsync(int sessionId)
    {
        try
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                return false;

            session.IsActive = false;
            return await _sessionRepository.UpdateAsync(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> RevokeAllSessionsExceptCurrentAsync(int userId, string currentToken)
    {
        try
        {
            var currentSession = await _sessionRepository.GetByTokenAsync(currentToken);
            if (currentSession == null)
                return false;

            var userSessions = await _sessionRepository.GetByUserIdAsync(userId);
            var sessionsToRevoke = userSessions
                .Where(s => s.Id != currentSession.Id && s.IsActive)
                .ToList();

            if (!sessionsToRevoke.Any())
                return true;

            foreach (var session in sessionsToRevoke)
            {
                session.IsActive = false;
                await _sessionRepository.UpdateAsync(session);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking sessions for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ValidateSessionAsync(string token)
    {
        try
        {
            var session = await _sessionRepository.GetByTokenAsync(token);

            if (session == null || !session.IsActive)
                return false;

            // Update last activity time
            return await UpdateSessionActivityAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session with token");
            return false;
        }
    }

    public async Task<bool> UpdateSessionActivityAsync(string token)
    {
        try
        {
            var session = await _sessionRepository.GetByTokenAsync(token);
            if (session == null || !session.IsActive)
                return false;

            session.LastActivityAt = DateTime.UtcNow;
            return await _sessionRepository.UpdateAsync(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity for session with token");
            return false;
        }
    }

    public async Task<List<UserSessionDto>> GetUserSessionsAsync(int userId)
    {
        try
        {
            var sessions = await _sessionRepository.GetByUserIdAsync(userId);
            return sessions.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserSessionDto> GetSessionByIdAsync(int sessionId)
    {
        try
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                throw new InvalidOperationException($"Session with ID {sessionId} not found");
                
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<UserSessionDto> GetSessionByTokenAsync(string token)
    {
        try
        {
            var session = await _sessionRepository.GetByTokenAsync(token);
            if (session == null)
                throw new InvalidOperationException($"Session with token not found");
                
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session by token");
            throw;
        }
    }

    private string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }
} 