using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Core.Services;

public class UserSessionService : IUserSessionService
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    
    public UserSessionService(IUserSessionRepository sessionRepository, ICurrentUserProvider currentUserProvider)
    {
        _sessionRepository = sessionRepository;
        _currentUserProvider = currentUserProvider;
    }
    
    public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync()
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        return await _sessionRepository.GetActiveSessionsByUserIdAsync(userId.Value);
    }
    
    public async Task<UserSessionDto> GetCurrentSessionAsync()
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var token = _currentUserProvider.GetCurrentToken();
        
        if (userId == null || string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        var session = await _sessionRepository.GetSessionByTokenAsync(userId.Value, token);
            
        if (session == null)
        {
            throw new InvalidOperationException("Current session not found");
        }
        
        return session;
    }
    
    public async Task<bool> TerminateSessionAsync(int sessionId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        return await _sessionRepository.DeactivateSessionAsync(sessionId, userId.Value);
    }
    
    public async Task<int> TerminateAllSessionsExceptCurrentAsync()
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var token = _currentUserProvider.GetCurrentToken();
        
        if (userId == null || string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        return await _sessionRepository.DeactivateAllSessionsExceptTokenAsync(userId.Value, token);
    }
    
    public async Task UpdateSessionActivityAsync()
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var token = _currentUserProvider.GetCurrentToken();
        
        if (userId == null || string.IsNullOrEmpty(token))
        {
            return; // Silently exit - user not authenticated
        }
        
        await _sessionRepository.UpdateSessionActivityAsync(userId.Value, token);
    }
} 