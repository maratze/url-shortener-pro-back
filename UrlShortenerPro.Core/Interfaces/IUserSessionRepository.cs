using UrlShortenerPro.Core.Dtos;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUserSessionRepository
{
    /// <summary>
    /// Get all active user sessions
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>Collection of user session DTOs</returns>
    Task<IEnumerable<UserSessionDto>> GetActiveSessionsByUserIdAsync(int userId);
    
    /// <summary>
    /// Get session by token and user identifier
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="token">Session token</param>
    /// <returns>User session DTO or null if session not found</returns>
    Task<UserSessionDto?> GetSessionByTokenAsync(int userId, string token);
    
    /// <summary>
    /// Get session by identifier and check if it belongs to the specified user
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="userId">User identifier</param>
    /// <returns>User session DTO or null if session not found</returns>
    Task<UserSessionDto?> GetSessionByIdAsync(int sessionId, int userId);
    
    /// <summary>
    /// Deactivate the specified session
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="userId">User identifier</param>
    /// <returns>true, if the session was successfully deactivated, otherwise false</returns>
    Task<bool> DeactivateSessionAsync(int sessionId, int userId);
    
    /// <summary>
    /// Deactivate all user sessions except the specified one
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="excludeToken">Session token to exclude</param>
    /// <returns>Number of deactivated sessions</returns>
    Task<int> DeactivateAllSessionsExceptTokenAsync(int userId, string excludeToken);
    
    /// <summary>
    /// Update the last activity time of the specified session
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="token">Session token</param>
    /// <returns>true, if the session was successfully updated, otherwise false</returns>
    Task<bool> UpdateSessionActivityAsync(int userId, string token);
} 