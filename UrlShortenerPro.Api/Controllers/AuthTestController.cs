using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/auth-test")]
public class AuthTestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok(new { message = "Public endpoint works!" });
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        var identity = HttpContext.User.Identity;
        var claims = HttpContext.User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

        return Ok(new { 
            message = "Protected endpoint works!", 
            isAuthenticated = identity?.IsAuthenticated ?? false,
            authenticationType = identity?.AuthenticationType,
            claims = claims,
            headers = headers
        });
    }
}