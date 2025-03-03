using Microsoft.AspNetCore.Mvc;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("")]
public class RedirectController(IUrlService urlService) : ControllerBase
{
    // GET /{shortCode}
    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectToUrl(string shortCode)
    {
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        string userAgent = Request.Headers["User-Agent"].ToString();
        string referer = Request.Headers["Referer"].ToString();
            
        string originalUrl = await urlService.RedirectAndTrackAsync(shortCode, ipAddress, userAgent, referer);
            
        if (string.IsNullOrEmpty(originalUrl))
            return NotFound();
                
        return Redirect(originalUrl);
    }
}