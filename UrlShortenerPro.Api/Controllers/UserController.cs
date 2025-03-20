using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(
    IUserService userService,
    IJwtService jwtService,
    ILogger<UserController> logger)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegistrationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userResponse = await userService.RegisterAsync(request);
            return Ok(userResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user registration");
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

            var userResponse = await userService.LoginAsync(request, deviceInfo, ipAddress, location);
            return Ok(userResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login");
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

            var user = await userService.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user profile");
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

            var updatedUser = await userService.UpdateProfileAsync(userId.Value, request);
            if (updatedUser == null)
            {
                return NotFound("User not found");
            }

            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, "An error occurred while updating the profile");
        }
    }

    [HttpPost("check-email")]
    public async Task<IActionResult> CheckEmailAvailability([FromBody] string email)
    {
        try
        {
            var isAvailable = await userService.IsEmailAvailableAsync(email);
            return Ok(new { IsAvailable = isAvailable });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking email availability");
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

            var userResponse = await userService.AuthenticateWithOAuthAsync(request, deviceInfo, ipAddress, location);
            return Ok(userResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during OAuth authentication");
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

            var success = await userService.ChangePasswordAsync(userId.Value, request);
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
            logger.LogError(ex, "Error changing password");
            return StatusCode(500, "An error occurred while changing the password");
        }
    }

    [HttpDelete("account")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Invalid token");
            }

            var success = await userService.DeleteUserAsync(userId.Value);
            if (!success)
            {
                return BadRequest("Failed to delete account");
            }

            return Ok(new { Message = "Account deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error during account deletion for user ID {UserId}", GetCurrentUserId());
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during account deletion for user ID {UserId}", GetCurrentUserId());
            return StatusCode(500, "An error occurred while deleting the account");
        }
    }

    // Helper method to get the current user ID from the token
    private int? GetCurrentUserId()
    {
        string authorizationHeader = HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            logger.LogWarning("No valid authorization header found");
            return null;
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                logger.LogWarning("User ID claim not found or invalid in token");
                return null;
            }
            return userId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user ID from token");
            return null;
        }
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
                logger.LogWarning("Не удалось получить ID пользователя из токена");
                return Unauthorized(new { message = "Недействительный токен" });
            }

            var user = await userService.UpgradeToPremiumAsync(userId);
            if (user == null)
            {
                logger.LogWarning("Пользователь с ID {UserId} не найден при попытке апгрейда", userId);
                return NotFound(new { message = "Пользователь не найден" });
            }

            logger.LogInformation("Пользователь с ID {UserId} успешно перешел на премиум", userId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при апгрейде пользователя до премиум");
            return StatusCode(500, new { message = "Произошла ошибка при апгрейде до премиум" });
        }
    }

    // Вспомогательный метод для валидации email
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

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

