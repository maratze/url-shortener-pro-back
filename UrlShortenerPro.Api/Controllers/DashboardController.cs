using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IAnalyticsService analyticsService,
            ILogger<DashboardController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] int days = 7)
        {
            try
            {
                // Получаем ID пользователя из токена
                if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                {
                    _logger.LogWarning("User ID claim not found or invalid in token");
                    return Unauthorized();
                }

                var stats = await _analyticsService.GetDashboardStatsAsync(userId, days);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
} 