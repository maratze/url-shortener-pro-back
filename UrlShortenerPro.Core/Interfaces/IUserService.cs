using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUserService
{
    Task<UserResponse> RegisterAsync(UserRegistrationRequest request);
    Task<UserResponse> LoginAsync(UserLoginRequest request);
    Task<UserResponse> GetByIdAsync(int id);
    Task<UserResponse> UpgradeToPremiumAsync(int userId);
    Task<bool> IsEmailAvailableAsync(string email);
}