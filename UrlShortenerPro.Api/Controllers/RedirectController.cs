using Microsoft.AspNetCore.Mvc;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
public class RedirectController(IUrlService urlService) : ControllerBase
{
    [HttpGet("/{shortCode}")]
    public new async Task<IActionResult> Redirect(string shortCode)
    {
        // Получаем IP, User-Agent и Referer из запроса
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        string referer = HttpContext.Request.Headers["Referer"].ToString();

        // Использование RedirectAndTrackAsync вместо отдельного вызова RecordClickAsync
        var originalUrl = await urlService.RedirectAndTrackAsync(shortCode, ipAddress, userAgent, referer);

        if (originalUrl == null)
        {
            return NotFound(new { message = "Short URL not found or expired" });
        }

        // Перенаправляем на оригинальный URL
        return await Redirect(originalUrl);
    }
}