using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Email and password are required");
                }

                // Get device and location info
                var deviceInfo = Request.Headers["User-Agent"].ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var location = "Unknown"; // Could add IP geolocation here

                var user = await _userService.LoginAsync(request, deviceInfo, ipAddress, location);
                
                _logger.LogInformation("User {Email} logged in successfully", request.Email);
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt");
                return StatusCode(500, "An error occurred during login");
            }
        }
    }
} 