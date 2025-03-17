using UrlShortenerPro.Core.Dtos;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUserRepository
{
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<UserDto> CreateAsync(UserDto user);
    Task<bool> UpdateAsync(UserDto user);
    Task<bool> DeleteAsync(int id);
    Task<int> GetTotalUserCountAsync();
    Task<int> GetActiveUserCountAsync();
} 