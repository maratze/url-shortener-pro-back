using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(IUserService userService) : ControllerBase
{
    // POST api/users/register
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register([FromBody] UserRegistrationRequest request)
    {
        try
        {
            var result = await userService.RegisterAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Произошла ошибка при регистрации пользователя" });
        }
    }

    // POST api/users/login
    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login([FromBody] UserLoginRequest request)
    {
        try
        {
            var result = await userService.LoginAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Произошла ошибка при входе в систему" });
        }
    }

    // GET api/users/me
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        var user = await userService.GetByIdAsync(userId);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // POST api/users/upgrade
    [HttpPost("upgrade")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> UpgradeToPremium()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        var user = await userService.UpgradeToPremiumAsync(userId);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // GET api/users/check-email
    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "Email не указан" });

        bool isAvailable = await userService.IsEmailAvailableAsync(email);
        return Ok(new { isAvailable });
    }
}