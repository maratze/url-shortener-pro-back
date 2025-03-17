using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserSessionController : ControllerBase
    {
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<UserSessionController> _logger;

        public UserSessionController(
            IUserSessionService userSessionService,
            ILogger<UserSessionController> logger)
        {
            _userSessionService = userSessionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserSessions()
        {
            try
            {
                // Get user ID from claims
                if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
                {
                    return Unauthorized();
                }

                var sessions = await _userSessionService.GetSessionsByUserIdAsync(userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user sessions");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentSession()
        {
            try
            {
                // Get user ID from claims
                if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
                {
                    return Unauthorized();
                }

                // Get token from Authorization header
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                
                var session = await _userSessionService.GetSessionByTokenAsync(token);
                if (session == null || session.UserId != userId)
                {
                    return NotFound();
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current session");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RevokeSession(int id)
        {
            try
            {
                // Get user ID from claims
                if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
                {
                    return Unauthorized();
                }

                // Get token from Authorization header
                string currentToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                
                // Check if the session belongs to the user
                var session = await _userSessionService.GetSessionByIdAsync(id);
                if (session == null || session.UserId != userId)
                {
                    return NotFound();
                }

                // Prevent revoking the current session
                var currentSession = await _userSessionService.GetSessionByTokenAsync(currentToken);
                if (currentSession != null && currentSession.Id == id)
                {
                    return BadRequest("Cannot revoke the current session. Please use the logout endpoint instead.");
                }

                await _userSessionService.RevokeSessionAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking session with ID: {SessionId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("all-except-current")]
        public async Task<IActionResult> RevokeAllSessionsExceptCurrent()
        {
            try
            {
                // Get user ID from claims
                if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
                {
                    return Unauthorized();
                }

                // Get token from Authorization header
                string currentToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                
                await _userSessionService.RevokeAllSessionsExceptCurrentAsync(userId, currentToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all sessions except current");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
} 