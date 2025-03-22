using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/user-sessions")]
[Authorize]
public class UserSessionController : ControllerBase
{
    private readonly IUserSessionService _sessionService;
    
    public UserSessionController(IUserSessionService sessionService)
    {
        _sessionService = sessionService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUserSessions()
    {
        try
        {
            var sessions = await _sessionService.GetUserSessionsAsync();
            return Ok(sessions);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving sessions", details = ex.Message });
        }
    }
    
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSession()
    {
        try
        {
            var session = await _sessionService.GetCurrentSessionAsync();
            return Ok(session);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving current session", details = ex.Message });
        }
    }
    
    [HttpDelete("{sessionId:int}")]
    public async Task<IActionResult> TerminateSession(int sessionId)
    {
        try
        {
            var result = await _sessionService.TerminateSessionAsync(sessionId);
            
            if (result)
            {
                return Ok(new { success = true, message = "Session terminated successfully" });
            }
            
            return NotFound(new { success = false, error = "Session not found or already terminated" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = "An error occurred while terminating session", details = ex.Message });
        }
    }
    
    [HttpDelete("terminate-all-except-current")]
    public async Task<IActionResult> TerminateAllSessionsExceptCurrent()
    {
        try
        {
            var count = await _sessionService.TerminateAllSessionsExceptCurrentAsync();
            
            return Ok(new 
            { 
                success = true, 
                message = count > 0 
                    ? $"Successfully terminated {count} session{(count == 1 ? "" : "s")}" 
                    : "No other active sessions found" 
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = "An error occurred while terminating sessions", details = ex.Message });
        }
    }
} 