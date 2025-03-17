using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortenerPro.Core.Dtos;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUserSessionService
{
    Task<UserSessionDto> CreateSessionAsync(int userId, string deviceInfo, string ipAddress, string location);
    Task<UserSessionDto> GetSessionByIdAsync(int sessionId);
    Task<UserSessionDto> GetSessionByTokenAsync(string token);
    Task<IEnumerable<UserSessionDto>> GetSessionsByUserIdAsync(int userId);
    Task<bool> RevokeSessionAsync(int sessionId);
    Task<bool> RevokeAllSessionsExceptCurrentAsync(int userId, string currentToken);
    Task<bool> ValidateSessionAsync(string token);
    Task<bool> UpdateSessionActivityAsync(string token);
    Task<List<UserSessionDto>> GetUserSessionsAsync(int userId);
} 