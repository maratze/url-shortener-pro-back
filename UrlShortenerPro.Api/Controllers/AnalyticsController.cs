using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IUrlService _urlService;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        IUrlService urlService)
    {
        _analyticsService = analyticsService;
        _urlService = urlService;
    }

    // GET api/analytics/{urlId}/clicks
    [HttpGet("{urlId}/clicks")]
    public async Task<IActionResult> GetClickStats(int urlId, [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        bool canAccess = await _analyticsService.UserCanAccessUrlAnalyticsAsync(urlId, userId);

        if (!canAccess)
            return Forbid();

        var stats = await _analyticsService.GetClickStatsAsync(urlId, startDate, endDate);
        return Ok(stats);
    }

    // GET api/analytics/{urlId}/devices
    [HttpGet("{urlId}/devices")]
    public async Task<IActionResult> GetDeviceStats(int urlId, [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        bool canAccess = await _analyticsService.UserCanAccessUrlAnalyticsAsync(urlId, userId);

        if (!canAccess)
            return Forbid();

        // Проверка премиум статуса для расширенной аналитики
        bool isPremium = await _analyticsService.UserCanAccessPremiumAnalyticsAsync(userId);
        if (!isPremium)
            return StatusCode(403, new { message = "Эта функция доступна только для премиум пользователей" });

        var stats = await _analyticsService.GetDeviceStatsAsync(urlId, startDate, endDate);
        return Ok(stats);
    }

    // GET api/analytics/{urlId}/locations
    [HttpGet("{urlId}/locations")]
    public async Task<IActionResult> GetLocationStats(int urlId, [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        bool canAccess = await _analyticsService.UserCanAccessUrlAnalyticsAsync(urlId, userId);

        if (!canAccess)
            return Forbid();

        // Проверка премиум статуса для расширенной аналитики
        bool isPremium = await _analyticsService.UserCanAccessPremiumAnalyticsAsync(userId);
        if (!isPremium)
            return StatusCode(403, new { message = "Эта функция доступна только для премиум пользователей" });

        var stats = await _analyticsService.GetLocationStatsAsync(urlId, startDate, endDate);
        return Ok(stats);
    }

    // GET api/analytics/{urlId}/referrers
    [HttpGet("{urlId}/referrers")]
    public async Task<IActionResult> GetReferrerStats(int urlId, [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException());
        bool canAccess = await _analyticsService.UserCanAccessUrlAnalyticsAsync(urlId, userId);

        if (!canAccess)
            return Forbid();

        // Проверка премиум статуса для расширенной аналитики
        bool isPremium = await _analyticsService.UserCanAccessPremiumAnalyticsAsync(userId);
        if (!isPremium)
            return StatusCode(403, new { message = "Эта функция доступна только для премиум пользователей" });

        var stats = await _analyticsService.GetReferrerStatsAsync(urlId, startDate, endDate);
        return Ok(stats);
    }
}