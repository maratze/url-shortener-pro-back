using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserService userService,
        IJwtService jwtService,
        ILogger<UserController> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegistrationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userResponse = await _userService.RegisterAsync(request);
            return Ok(userResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, "An error occurred during registration");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get device and location info
            var deviceInfo = Request.Headers.UserAgent.ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var location = "Unknown"; // Could add IP geolocation here

            var userResponse = await _userService.LoginAsync(request, deviceInfo, ipAddress, location);
            return Ok(userResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "An error occurred during login");
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Invalid token");
            }

            var user = await _userService.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, "An error occurred while retrieving the profile");
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Invalid token");
            }

            var updatedUser = await _userService.UpdateProfileAsync(userId.Value, request);
            if (updatedUser == null)
            {
                return NotFound("User not found");
            }

            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, "An error occurred while updating the profile");
        }
    }

    [HttpPost("check-email")]
    public async Task<IActionResult> CheckEmailAvailability([FromBody] string email)
    {
        try
        {
            var isAvailable = await _userService.IsEmailAvailableAsync(email);
            return Ok(new { IsAvailable = isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability");
            return StatusCode(500, "An error occurred while checking email availability");
        }
    }

    [HttpPost("oauth")]
    public async Task<IActionResult> AuthenticateWithOAuth(OAuthRequest request)
    {
        try
        {
            // Get device and location info
            var deviceInfo = Request.Headers.UserAgent.ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var location = "Unknown"; // Could add IP geolocation here

            var userResponse = await _userService.AuthenticateWithOAuthAsync(request, deviceInfo, ipAddress, location);
            return Ok(userResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth authentication");
            return StatusCode(500, "An error occurred during authentication");
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Invalid token");
            }

            var success = await _userService.ChangePasswordAsync(userId.Value, request);
            if (!success)
            {
                return BadRequest("Failed to change password");
            }

            return Ok(new { Message = "Password changed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, "An error occurred while changing the password");
        }
    }

    // Helper method to get the current user ID from the token
    private int? GetCurrentUserId()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        return _jwtService.GetUserIdFromToken(token);
    }

    // POST api/users/upgrade
    [HttpPost("upgrade")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> UpgradeToPremium()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Не удалось получить ID пользователя из токена");
                return Unauthorized(new { message = "Недействительный токен" });
            }

            var user = await _userService.UpgradeToPremiumAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с ID {UserId} не найден при попытке апгрейда", userId);
                return NotFound(new { message = "Пользователь не найден" });
            }

            _logger.LogInformation("Пользователь с ID {UserId} успешно перешел на премиум", userId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при апгрейде пользователя до премиум");
            return StatusCode(500, new { message = "Произошла ошибка при апгрейде до премиум" });
        }
    }

    // Вспомогательный метод для валидации email
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        try
        {
            // Использование встроенного EmailAddressAttribute для валидации
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
                return false;

            // Дополнительная проверка с использованием регулярного выражения
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
}

