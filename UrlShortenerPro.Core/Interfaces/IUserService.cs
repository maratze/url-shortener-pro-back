using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUserService
{
    Task<UserResponse> RegisterAsync(UserRegistrationRequest request);
    Task<UserResponse> LoginAsync(UserLoginRequest request);
    Task<UserResponse> GetByIdAsync(int id);
    Task<UserResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<UserResponse> UpgradeToPremiumAsync(int userId);
    Task<bool> IsEmailAvailableAsync(string email);
    Task<UserResponse> AuthenticateWithOAuthAsync(OAuthRequest request);
}