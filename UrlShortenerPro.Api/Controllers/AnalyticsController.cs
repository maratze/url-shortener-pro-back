using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Api.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [HttpGet("url/{urlId}")]
        public async Task<IActionResult> GetUrlAnalytics(int urlId)
        {
            try
            {
                // Get user ID from claims
                if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                {
                    _logger.LogWarning("User ID claim not found or invalid in token");
                    return Unauthorized();
                }

                var analytics = await _analyticsService.GetUrlAnalyticsAsync(urlId, userId);
                return Ok(analytics);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics for URL with ID: {UrlId}", urlId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserAnalytics()
        {
            try
            {
                // Get user ID from claims
                if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                {
                    _logger.LogWarning("User ID claim not found or invalid in token");
                    return Unauthorized();
                }

                var analytics = await _analyticsService.GetUserAnalyticsAsync(userId);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user analytics");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("url/{urlId}/clicks")]
        public async Task<IActionResult> GetUrlClicks(int urlId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                // Get user ID from claims
                if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
                {
                    return Unauthorized();
                }

                var clicks = await _analyticsService.GetUrlClicksAsync(urlId, userId, startDate, endDate);
                return Ok(clicks);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clicks for URL with ID: {UrlId}", urlId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("url/{urlId}/devices")]
        public async Task<IActionResult> GetDeviceStats(int urlId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                // Get user ID from claims
                if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
                {
                    return Unauthorized();
                }

                // Check if user can access URL analytics
                bool canAccess = await _analyticsService.UserCanAccessUrlAnalyticsAsync(urlId, userId);
                if (!canAccess)
                {
                    return Forbid();
                }

                // Check if user has premium access
                bool isPremium = await _analyticsService.UserCanAccessPremiumAnalyticsAsync(userId);
                if (!isPremium)
                {
                    return StatusCode(403, "This feature is only available for premium users.");
                }

                var stats = await _analyticsService.GetDeviceStatsAsync(urlId, startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device stats for URL with ID: {UrlId}", urlId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("url/{urlId}/locations")]
        public async Task<IActionResult> GetLocationStats(int urlId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                // Get user ID from claims
                if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
                {
                    return Unauthorized();
                }

                // Check if user can access URL analytics
                bool canAccess = await _analyticsService.UserCanAccessUrlAnalyticsAsync(urlId, userId);
                if (!canAccess)
                {
                    return Forbid();
                }

                // Check if user has premium access
                bool isPremium = await _analyticsService.UserCanAccessPremiumAnalyticsAsync(userId);
                if (!isPremium)
                {
                    return StatusCode(403, "This feature is only available for premium users.");
                }

                var stats = await _analyticsService.GetLocationStatsAsync(urlId, startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location stats for URL with ID: {UrlId}", urlId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("url/{urlId}/referrers")]
        public async Task<IActionResult> GetReferrerStats(int urlId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                // Get user ID from claims
                if (!int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
                {
                    return Unauthorized();
                }

                // Check if user can access URL analytics
                bool canAccess = await _analyticsService.UserCanAccessUrlAnalyticsAsync(urlId, userId);
                if (!canAccess)
                {
                    return Forbid();
                }

                // Check if user has premium access
                bool isPremium = await _analyticsService.UserCanAccessPremiumAnalyticsAsync(userId);
                if (!isPremium)
                {
                    return StatusCode(403, "This feature is only available for premium users.");
                }

                var stats = await _analyticsService.GetReferrerStatsAsync(urlId, startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting referrer stats for URL with ID: {UrlId}", urlId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}