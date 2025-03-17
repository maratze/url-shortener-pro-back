using UrlShortenerPro.Core.Dtos;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUserSessionRepository
{
    Task<UserSessionDto> CreateAsync(UserSessionDto sessionDto);
    Task<UserSessionDto?> GetByIdAsync(int id);
    Task<UserSessionDto?> GetByTokenAsync(string token);
    Task<IEnumerable<UserSessionDto>> GetByUserIdAsync(int userId);
    Task<bool> UpdateAsync(UserSessionDto sessionDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAllExceptAsync(int userId, int sessionIdToKeep);
} 