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
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // POST api/users/register
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register([FromBody] UserRegistrationRequest request)
    {
        try
        {
            // Валидация email
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Email не указан" });
            }

            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { message = "Указан некорректный формат email" });
            }

            // Валидация пароля
            if (string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Пароль не указан" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "Пароль должен содержать минимум 6 символов" });
            }

            var result = await _userService.RegisterAsync(request);
            _logger.LogInformation("Пользователь с email {Email} успешно зарегистрирован", request.Email);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Ошибка при регистрации: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка при регистрации пользователя с email {Email}", request.Email);
            return StatusCode(500, new { message = "Произошла ошибка при регистрации пользователя" });
        }
    }

    // POST api/users/login
    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login([FromBody] UserLoginRequest request)
    {
        try
        {
            // Валидация email
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Email не указан" });
            }

            // Валидация пароля
            if (string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Пароль не указан" });
            }

            var result = await _userService.LoginAsync(request);
            _logger.LogInformation("Пользователь с email {Email} успешно вошел в систему", request.Email);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Ошибка при входе: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка при входе пользователя с email {Email}", request.Email);
            return StatusCode(500, new { message = "Произошла ошибка при входе в систему" });
        }
    }

    // GET api/users/me
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Не удалось получить ID пользователя из токена");
                return Unauthorized(new { message = "Недействительный токен" });
            }

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с ID {UserId} не найден", userId);
                return NotFound(new { message = "Пользователь не найден" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении данных текущего пользователя");
            return StatusCode(500, new { message = "Произошла ошибка при получении данных пользователя" });
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

    // GET api/users/check-email
    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email не указан" });
            }

            if (!IsValidEmail(email))
            {
                return BadRequest(new { message = "Указан некорректный формат email" });
            }

            bool isAvailable = await _userService.IsEmailAvailableAsync(email);
            return Ok(new { isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке доступности email {Email}", email);
            return StatusCode(500, new { message = "Произошла ошибка при проверке доступности email" });
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