using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortenerPro.Api.DTOs;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/url")]
public class UrlController : ControllerBase
{
    private readonly IUrlService _urlService;
    private readonly ILogger<UrlController> _logger;
    private readonly IClientTrackingService _clientTrackingService;

    public UrlController(
        IUrlService urlService,
        ILogger<UrlController> logger,
        IClientTrackingService clientTrackingService)
    {
        _urlService = urlService;
        _logger = logger;
        _clientTrackingService = clientTrackingService;
    }

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> GetByShortCode(string shortCode)
    {
        try
        {
            var url = await _urlService.GetByShortCodeAsync(shortCode);
            if (url == null)
            {
                return NotFound();
            }

            return Ok(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting URL by short code: {ShortCode}", shortCode);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("redirect/{shortCode}")]
    public async Task<IActionResult> RedirectToOriginal(string shortCode)
    {
        try
        {
            // Get request information
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string userAgent = Request.Headers["User-Agent"].ToString();
            string referer = Request.Headers["Referer"].ToString();
            
            var originalUrl = await _urlService.GetOriginalUrlAndTrackClickAsync(shortCode, ipAddress, userAgent, referer);
            if (string.IsNullOrEmpty(originalUrl))
            {
                return NotFound();
            }

            return Redirect(originalUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redirecting to original URL for short code: {ShortCode}", shortCode);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UrlDto urlDto)
    {
        try
        {
            // Get client IP address for tracking
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // Check if anonymous user has remaining free requests
            if (urlDto.UserId == null)
            {
                int remainingRequests = await _clientTrackingService.GetRemainingFreeRequestsAsync(clientIp);
                if (remainingRequests <= 0)
                {
                    return StatusCode(429, "You have reached the maximum number of free requests for this month. Please sign up for an account to continue.");
                }
            }

            var createdUrl = await _urlService.CreateAsync(urlDto);
            return CreatedAtAction(nameof(GetByShortCode), new { shortCode = createdUrl.ShortCode }, createdUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shortened URL");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [Authorize]
    [HttpGet("user")]
    public async Task<IActionResult> GetByUserId()
    {
        try
        {
            // Get user ID from claims
            if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
            {
                return Unauthorized();
            }

            var urls = await _urlService.GetByUserIdAsync(userId);
            return Ok(urls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting URLs by user ID");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UrlDto urlDto)
    {
        try
        {
            // Get user ID from claims
            if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
            {
                return Unauthorized();
            }

            // Ensure the URL belongs to the user
            var existingUrl = await _urlService.GetByIdAsync(id);
            if (existingUrl == null)
            {
                return NotFound();
            }

            if (existingUrl.UserId != userId)
            {
                return Forbid();
            }

            urlDto.Id = id;
            urlDto.UserId = userId;
            var updatedUrl = await _urlService.UpdateAsync(urlDto);
            return Ok(updatedUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating URL with ID: {UrlId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // Get user ID from claims
            if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
            {
                return Unauthorized();
            }

            // Ensure the URL belongs to the user
            var existingUrl = await _urlService.GetByIdAsync(id);
            if (existingUrl == null)
            {
                return NotFound();
            }

            if (existingUrl.UserId != userId)
            {
                return Forbid();
            }

            await _urlService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting URL with ID: {UrlId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
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
            int remainingRequests = await _clientTrackingService.GetRemainingFreeRequestsAsync(clientId);

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