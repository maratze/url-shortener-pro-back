using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortenerPro.Api.DTOs;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/urls")]
public class UrlController(IUrlService urlService, IClientTrackingService clientTrackingService) : ControllerBase
{
    // POST api/urls
    [HttpPost]
    public async Task<ActionResult<UrlResponse>> CreateShortUrl([FromBody] CreateUrlRequest request)
    {
        try
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                   throw new InvalidOperationException());
            }

            var urlRequest = new UrlCreationRequest
            {
                OriginalUrl = request.OriginalUrl,
                CustomCode = request.CustomCode,
                ExpiresInDays = request.ExpiresInDays,
                UserId = userId
            };

            var result = await urlService.CreateShortUrlAsync(urlRequest);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Произошла ошибка при создании короткой ссылки" });
        }
    }

    // GET api/urls/{shortCode}
    [HttpGet("{shortCode}")]
    public async Task<ActionResult<UrlResponse>> GetUrl(string shortCode)
    {
        var url = await urlService.GetUrlByShortCodeAsync(shortCode);
        if (url == null)
        {
            return NotFound();
        }

        // Проверка доступа - только владелец или публичный доступ
        if (url.UserId.HasValue)
        {
            if (User.Identity?.IsAuthenticated == false ||
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException()) !=
                url.UserId.Value)
            {
                return Forbid();
            }
        }

        return Ok(url);
    }

    // GET api/urls
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UrlResponse>>> GetMyUrls()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        var urls = await urlService.GetUrlsByUserIdAsync(userId);
        return Ok(urls);
    }

    // DELETE api/urls/{shortCode}
    [HttpDelete("{shortCode}")]
    [Authorize]
    public async Task<IActionResult> DeleteUrl(string shortCode)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        bool result = await urlService.DeleteUrlAsync(shortCode, userId);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("remaining-requests")]
    public async Task<IActionResult> GetRemainingRequests()
    {
        try
        {
            // Extract client ID from header
            if (!Request.Headers.TryGetValue("X-Client-Id", out var clientIdValues))
            {
                return BadRequest(new { message = "Client ID is required" });
            }

            string clientId = clientIdValues.ToString();

            // Get remaining requests from service
            int remainingRequests = await clientTrackingService.GetRemainingFreeRequestsAsync(clientId);

            return Ok(new { remainingRequests });
        }
        catch (Exception ex)
        {
            // Log exception
            return StatusCode(500,
                new { message = "An error occurred while retrieving remaining requests", error = ex.Message });
        }
    }
}