using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("")]
public class RedirectController : ControllerBase
{
    private readonly IUrlService _urlService;
    private readonly ILogger<RedirectController> _logger;

    public RedirectController(IUrlService urlService, ILogger<RedirectController> logger)
    {
        _urlService = urlService;
        _logger = logger;
    }

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
    {
        try
        {
            if (string.IsNullOrEmpty(shortCode))
            {
                return BadRequest("Short code is required");
            }

            // Make sure the short code is valid (only contains allowed characters)
            if (!Regex.IsMatch(shortCode, "^[a-zA-Z0-9_-]+$"))
            {
                return BadRequest("Invalid short code format");
            }

            // Get URL data
            var url = await _urlService.GetUrlByShortCodeAsync(shortCode);
            if (url == null)
            {
                return NotFound("URL not found or has expired");
            }

            // Track click with analytics data
            var trackingData = new ClickTrackingData
            {
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = Request.Headers.UserAgent.ToString(),
                ReferrerUrl = Request.Headers.Referer.ToString(),
                DeviceType = GetDeviceType(Request.Headers.UserAgent.ToString()),
                Browser = GetBrowser(Request.Headers.UserAgent.ToString()),
                OperatingSystem = GetOperatingSystem(Request.Headers.UserAgent.ToString()),
                Country = "Unknown", // In a real app, could use IP geolocation
                City = "Unknown"
            };

            await _urlService.TrackClickAsync(shortCode, trackingData);

            // Redirect to the original URL
            return Redirect(url.OriginalUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redirecting short code {ShortCode}", shortCode);
            return StatusCode(500, "An error occurred while processing the redirect");
        }
    }

    // Simple device type detection
    private string GetDeviceType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") && !userAgent.Contains("Tablet"))
            return "Mobile";
        if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
            return "Tablet";
        return "Desktop";
    }

    // Simple browser detection
    private string GetBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        if (userAgent.Contains("Edge") || userAgent.Contains("Edg/"))
            return "Edge";
        if (userAgent.Contains("Chrome") && !userAgent.Contains("Chromium"))
            return "Chrome";
        if (userAgent.Contains("Firefox"))
            return "Firefox";
        if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome") && !userAgent.Contains("Chromium"))
            return "Safari";
        if (userAgent.Contains("MSIE") || userAgent.Contains("Trident/"))
            return "Internet Explorer";
        return "Other";
    }

    // Simple OS detection
    private string GetOperatingSystem(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        if (userAgent.Contains("Windows"))
            return "Windows";
        if (userAgent.Contains("Mac"))
            return "macOS";
        if (userAgent.Contains("Linux") && !userAgent.Contains("Android"))
            return "Linux";
        if (userAgent.Contains("Android"))
            return "Android";
        if (userAgent.Contains("iOS") || userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
            return "iOS";
        return "Other";
    }
}