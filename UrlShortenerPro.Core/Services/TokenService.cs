using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Core.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly IUserSessionService _sessionService;
    
    public TokenService(IConfiguration configuration, ILogger<TokenService> logger, IUserSessionService sessionService)
    {
        _configuration = configuration;
        _logger = logger;
        _sessionService = sessionService;
    }

    public async Task<string> GenerateTokenAsync(UserDto user, string deviceInfo, string ipAddress, string location)
    {
        try
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            
            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT configuration is missing or invalid");
            }

            // Create a new session for the user
            var session = await _sessionService.CreateSessionAsync(user.Id, deviceInfo, ipAddress, location);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("token", session.Token ?? string.Empty)
            };
            
            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for user {UserId}", user.Id);
            throw;
        }
    }
    
    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            // Check if token exists in active sessions
            var session = await GetSessionByTokenAsync(token);
            if (session == null || !session.IsActive)
            {
                return false;
            }

            // Validate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Jwt:Key"];
            
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT key is missing");
            }
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            // Update last activity time
            await _sessionService.UpdateSessionActivityAsync(token);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return false;
        }
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        try
        {
            var session = await GetSessionByTokenAsync(token);
            if (session == null)
            {
                return false;
            }

            return await _sessionService.RevokeSessionAsync(session.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return false;
        }
    }

    public async Task<UserSessionDto?> GetSessionByTokenAsync(string token)
    {
        try
        {
            return await _sessionService.GetSessionByTokenAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session by token");
            return null;
        }
    }
} 