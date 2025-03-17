using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Core.Interfaces;

public interface IUserService
{
    Task<UserResponse> RegisterAsync(UserRegistrationRequest request);
    Task<UserResponse> LoginAsync(UserLoginRequest request, string deviceInfo, string ipAddress, string location);
    Task<UserResponse> GetByIdAsync(int id);
    Task<UserResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<UserResponse> UpgradeToPremiumAsync(int userId);
    Task<bool> IsEmailAvailableAsync(string email);
    Task<UserResponse> AuthenticateWithOAuthAsync(OAuthRequest request, string deviceInfo, string ipAddress, string location);
}