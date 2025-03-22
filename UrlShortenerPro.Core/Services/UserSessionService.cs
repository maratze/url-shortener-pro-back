using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Core.Services;

public class UserSessionService(IUserSessionRepository sessionRepository, ICurrentUserProvider currentUserProvider)
    : IUserSessionService
{
    public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync()
    {
        var userId = currentUserProvider.GetCurrentUserId();
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        return await sessionRepository.GetActiveSessionsByUserIdAsync(userId.Value);
    }
    
    public async Task<UserSessionDto> GetCurrentSessionAsync()
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var token = currentUserProvider.GetCurrentToken();
        
        if (userId == null || string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        var session = await sessionRepository.GetSessionByTokenAsync(userId.Value, token);
            
        if (session == null)
        {
            throw new InvalidOperationException("Current session not found");
        }
        
        return session;
    }
    
    public async Task<bool> TerminateSessionAsync(int sessionId)
    {
        var userId = currentUserProvider.GetCurrentUserId();
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        return await sessionRepository.DeactivateSessionAsync(sessionId, userId.Value);
    }
    
    public async Task<int> TerminateAllSessionsExceptCurrentAsync()
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var token = currentUserProvider.GetCurrentToken();
        
        if (userId == null || string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        return await sessionRepository.DeactivateAllSessionsExceptTokenAsync(userId.Value, token);
    }
    
    public async Task UpdateSessionActivityAsync()
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var token = currentUserProvider.GetCurrentToken();
        
        if (userId == null || string.IsNullOrEmpty(token))
        {
            return; // Silently exit - user not authenticated
        }
        
        await sessionRepository.UpdateSessionActivityAsync(userId.Value, token);
    }
} 