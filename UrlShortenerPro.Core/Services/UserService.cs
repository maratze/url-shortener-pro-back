using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<UserResponse> RegisterAsync(UserRegistrationRequest request)
    {
        try
        {
            // Check if email already exists
            bool emailExists = await _userRepository.EmailExistsAsync(request.Email);
            if (emailExists)
            {
                throw new InvalidOperationException("Email is already registered");
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new UserDto
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                IsPremium = false,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            // Save user to database
            var createdUser = await _userRepository.CreateAsync(user);
            if (createdUser == null)
            {
                throw new InvalidOperationException("Failed to create user");
            }

            // For registration, use basic device info
            string deviceInfo = "Registration";
            string ipAddress = "Unknown";
            string location = "Unknown";

            // Generate token with session info
            string token = GenerateJwtToken(createdUser, deviceInfo, ipAddress, location);

            return new UserResponse
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                IsPremium = createdUser.IsPremium,
                CreatedAt = createdUser.CreatedAt,
                Token = token
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Email}", request.Email);
            throw new InvalidOperationException("An error occurred during registration");
        }
    }

    public async Task<UserResponse> LoginAsync(UserLoginRequest request, string deviceInfo, string ipAddress, string location)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;

            // Generate token with session info
            string token = GenerateJwtToken(user, deviceInfo, ipAddress, location);

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsPremium = user.IsPremium,
                CreatedAt = user.CreatedAt,
                Token = token
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            throw new InvalidOperationException("An error occurred during login");
        }
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsPremium = user.IsPremium,
                CreatedAt = user.CreatedAt,
                Token = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            throw new InvalidOperationException("An error occurred while retrieving user data");
        }
    }

    public async Task<UserResponse?> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            // Проверка и очистка входных данных
            string? firstName = request.FirstName?.Trim();
            string? lastName = request.LastName?.Trim();

            // Ограничение длины полей
            if (firstName?.Length > 50)
            {
                firstName = firstName.Substring(0, 50);
            }

            if (lastName?.Length > 50)
            {
                lastName = lastName.Substring(0, 50);
            }

            // Update user data
            user.FirstName = firstName;
            user.LastName = lastName;

            // Сохраняем изменения в базу данных
            bool updated = await _userRepository.UpdateAsync(user);
            if (!updated)
            {
                _logger.LogWarning("Failed to update profile for user {UserId}", userId);
                throw new InvalidOperationException("Failed to update user profile");
            }

            _logger.LogInformation("Profile updated for user {UserId}", userId);

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsPremium = user.IsPremium,
                CreatedAt = user.CreatedAt,
                Token = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
            throw new InvalidOperationException("An error occurred while updating the profile");
        }
    }

    public async Task<UserResponse> UpgradeToPremiumAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            user.IsPremium = true;

            // Сохраняем изменения в базу данных
            bool updated = await _userRepository.UpdateAsync(user);
            if (!updated)
            {
                _logger.LogWarning("Failed to upgrade user {UserId} to premium", userId);
                throw new InvalidOperationException("Failed to upgrade to premium");
            }

            _logger.LogInformation("User {UserId} upgraded to premium", userId);

            // For status update, use basic info
            string deviceInfo = "Status update";
            string ipAddress = "Unknown";
            string location = "Unknown";

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsPremium = user.IsPremium,
                CreatedAt = user.CreatedAt,
                Token = GenerateJwtToken(user, deviceInfo, ipAddress, location)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading user {UserId} to premium", userId);
            throw new InvalidOperationException("An error occurred while upgrading to premium");
        }
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        try
        {
            return !await _userRepository.EmailExistsAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability for {Email}", email);
            throw new InvalidOperationException("An error occurred while checking email availability");
        }
    }

    public async Task<UserResponse> AuthenticateWithOAuthAsync(OAuthRequest request, string deviceInfo, string ipAddress, string location)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                throw new InvalidOperationException("Email not provided by OAuth provider");
            }

            // Check if user with this email exists
            var user = await _userRepository.GetByEmailAsync(request.Email);
            bool isNewUser = user == null;

            // If user doesn't exist, register a new one
            if (isNewUser)
            {
                user = new UserDto
                {
                    Email = request.Email,
                    // Generate a random password that the user won't use
                    // (since they'll be logging in via OAuth)
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    // Используем данные из Google
                    FirstName = request.FirstName ?? request.Name,
                    LastName = request.LastName,
                    IsPremium = false,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                // Создаем пользователя в базе данных
                user = await _userRepository.CreateAsync(user);
                if (user == null)
                {
                    throw new InvalidOperationException("Failed to create user from OAuth");
                }

                _logger.LogInformation("Created new user {Email} via {Provider} OAuth", user.Email, request.Provider);
            }
            else
            {
                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                
                // Обновляем данные пользователя, если они пришли из Google
                bool userUpdated = false;
                
                // Если имя не было установлено ранее, но предоставлено Google
                if (string.IsNullOrEmpty(user.FirstName) && 
                    (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.Name)))
                {
                    user.FirstName = request.FirstName ?? request.Name;
                    userUpdated = true;
                }
                
                // Если фамилия не была установлена ранее, но предоставлена Google
                if (string.IsNullOrEmpty(user.LastName) && !string.IsNullOrEmpty(request.LastName))
                {
                    user.LastName = request.LastName;
                    userUpdated = true;
                }
                
                // Сохраняем изменения, если были обновления
                if (userUpdated)
                {
                    _logger.LogInformation("Updated user {Email} profile data from OAuth provider", user.Email);
                    await _userRepository.UpdateAsync(user);
                }
                else
                {
                    await _userRepository.UpdateAsync(user);
                }
            }

            // Generate token with session info
            string token = GenerateJwtToken(user, deviceInfo, ipAddress, location);

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsPremium = user.IsPremium,
                CreatedAt = user.CreatedAt,
                Token = token
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth authentication for {Email}", request.Email);
            throw new InvalidOperationException("An error occurred during OAuth authentication");
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
            {
                throw new InvalidOperationException("Current and new passwords must be provided");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Verify current password
            bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isCurrentPasswordValid)
            {
                throw new InvalidOperationException("Current password is incorrect");
            }

            // Hash new password
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordHash = newPasswordHash;
            
            // Сохраняем изменения в базу данных
            bool updated = await _userRepository.UpdateAsync(user);
            if (!updated)
            {
                _logger.LogWarning("Failed to update password for user {UserId}", userId);
                return false;
            }

            _logger.LogInformation("Password changed for user {UserId}", userId);
            return true;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw new InvalidOperationException("An error occurred while changing the password");
        }
    }

    private string GenerateJwtToken(UserDto user, string deviceInfo, string ipAddress, string location)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JwtSettings:Key not found"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("DeviceInfo", deviceInfo),
                new Claim("IPAddress", ipAddress),
                new Claim("Location", location)
            ]),
            Expires = DateTime.UtcNow.AddDays(7), // Token valid for 7 days
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}