using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;
using UrlShortenerPro.Infrastructure.Interfaces;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Core.Services;

public class UserService(IUserRepository userRepository, IConfiguration configuration)
    : IUserService
{
    public async Task<UserResponse> RegisterAsync(UserRegistrationRequest request)
    {
        // Проверка существования email
        bool emailExists = await userRepository.EmailExistsAsync(request.Email);
        if (emailExists)
        {
            throw new InvalidOperationException("Этот email уже зарегистрирован");
        }

        // Хэширование пароля
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            IsPremium = false,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        // Генерация токена
        string token = GenerateJwtToken(user);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            IsPremium = user.IsPremium,
            CreatedAt = user.CreatedAt,
            Token = token
        };
    }

    public async Task<UserResponse> LoginAsync(UserLoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new InvalidOperationException("Неверный email или пароль");
        }

        // Проверка пароля
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new InvalidOperationException("Неверный email или пароль");
        }

        // Обновление времени последнего входа
        user.LastLoginAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);
        await userRepository.SaveChangesAsync();

        // Генерация токена
        string token = GenerateJwtToken(user);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            IsPremium = user.IsPremium,
            CreatedAt = user.CreatedAt,
            Token = token
        };
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null)
            return null;

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            IsPremium = user.IsPremium,
            CreatedAt = user.CreatedAt,
            Token = null
        };
    }

    public async Task<UserResponse> UpgradeToPremiumAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        user.IsPremium = true;
        await userRepository.UpdateAsync(user);
        await userRepository.SaveChangesAsync();

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            IsPremium = user.IsPremium,
            CreatedAt = user.CreatedAt,
            Token = GenerateJwtToken(user)
        };
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return !await userRepository.EmailExistsAsync(email);
    }

    // Вспомогательный метод для генерации JWT токена
    private string GenerateJwtToken(User user)
    {
        var jwtKey = configuration["JwtSettings:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            jwtKey = "default_development_key_that_is_at_least_32_chars";
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("premium", user.IsPremium.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"] ?? "UrlShortenerPro",
            audience: configuration["JwtSettings:Audience"] ?? "UrlShortenerUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}