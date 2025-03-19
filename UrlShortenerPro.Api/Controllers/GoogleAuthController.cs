using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserService _userService;
    private readonly ILogger<GoogleAuthController> _logger;
    private readonly IConfiguration _configuration;

    public GoogleAuthController(
        IGoogleAuthService googleAuthService,
        IUserService userService,
        ILogger<GoogleAuthController> logger,
        IConfiguration configuration)
    {
        _googleAuthService = googleAuthService;
        _userService = userService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin([FromQuery] string returnUrl = "/")
    {
        // Создаем state для защиты от CSRF атак, включая returnUrl
        var state = $"{returnUrl}";
        var redirectUri = _configuration["GoogleOAuth:RedirectUri"];
        
        var authorizationUrl = _googleAuthService.GetAuthorizationUrl(redirectUri, state);
        
        return Redirect(authorizationUrl);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code))
        {
            _logger.LogWarning("Google callback received without code");
            return BadRequest("Authorization code is missing");
        }
        
        try
        {
            var redirectUri = _configuration["GoogleOAuth:RedirectUri"];
            
            // Обмениваем код на токен
            var tokenResponse = await _googleAuthService.ExchangeCodeForTokenAsync(code, redirectUri);
            
            // Получаем информацию о пользователе
            var userInfo = await _googleAuthService.GetUserInfoAsync(tokenResponse.Access_token);
            
            if (string.IsNullOrEmpty(userInfo.Email))
            {
                _logger.LogWarning("Google user info does not contain email");
                return BadRequest("Email is required from Google account");
            }
            
            _logger.LogInformation("Получены данные пользователя от Google: Email: {Email}, Name: {Name}", 
                userInfo.Email, userInfo.Name);

            // Аутентифицируем пользователя через OAuth
            var oauthRequest = new OAuthRequest
            {
                Provider = "google",
                Email = userInfo.Email,
                Token = tokenResponse.Access_token,
                FirstName = userInfo.Given_name,
                LastName = userInfo.Family_name,
                Name = userInfo.Name,
                Picture = userInfo.Picture
            };
            
            var deviceInfo = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var location = "Unknown"; // Можно реализовать геолокацию по IP
            
            var userResponse = await _userService.AuthenticateWithOAuthAsync(oauthRequest, deviceInfo, ipAddress, location);
            
            // Убедимся, что userResponse содержит необходимые данные
            if (userResponse == null || string.IsNullOrEmpty(userResponse.Token) || string.IsNullOrEmpty(userResponse.Email))
            {
                _logger.LogError("Ошибка при создании ответа авторизации: Token или Email отсутствуют");
                return Redirect($"{GetFrontendBaseUrl()}/login?error=Authentication+failed");
            }
            
            // Извлекаем returnUrl из state
            var returnUrl = state ?? "/dashboard";
            
            // Создаем URL для фронтенда со всеми необходимыми параметрами
            var frontendCallbackUrl = $"{GetFrontendBaseUrl()}/auth/callback" +
                $"?token={Uri.EscapeDataString(userResponse.Token)}" +
                $"&email={Uri.EscapeDataString(userResponse.Email)}" +
                $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
            
            _logger.LogInformation("Перенаправление на URL: {CallbackUrl}", frontendCallbackUrl);
            
            return Redirect(frontendCallbackUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return Redirect($"{GetFrontendBaseUrl()}/login?error=Authentication+failed");
        }
    }
    
    private string GetFrontendBaseUrl()
    {
        return _configuration["WebAppBaseUrl"]!;
    }
} 