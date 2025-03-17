using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using UrlShortenerPro.Core.Dtos;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Core.Services;

public class UrlService : IUrlService
{
    private readonly IUrlRepository _urlRepository;
    private readonly IClickDataRepository _clickDataRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UrlService> _logger;
    private readonly Random _random = new();

    public UrlService(
        IUrlRepository urlRepository,
        IClickDataRepository clickDataRepository,
        IConfiguration configuration,
        ILogger<UrlService> logger)
    {
        _urlRepository = urlRepository;
        _clickDataRepository = clickDataRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<UrlDto> GetByIdAsync(int id)
    {
        try
        {
            var url = await _urlRepository.GetByIdAsync(id);
            if (url == null)
            {
                throw new InvalidOperationException($"URL with ID {id} not found");
            }
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving URL with ID {UrlId}", id);
            throw;
        }
    }

    public async Task<UrlDto> GetByShortCodeAsync(string shortCode)
    {
        try
        {
            var url = await _urlRepository.GetByShortCodeAsync(shortCode);
            if (url == null)
            {
                throw new InvalidOperationException($"URL with short code {shortCode} not found");
            }
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving URL with short code {ShortCode}", shortCode);
            throw;
        }
    }

    public async Task<IEnumerable<UrlDto>> GetByUserIdAsync(int userId)
    {
        try
        {
            return await _urlRepository.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving URLs for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UrlDto> CreateAsync(UrlDto urlDto)
    {
        try
        {
            return await _urlRepository.CreateAsync(urlDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating URL {ShortCode}", urlDto.ShortCode);
            throw;
        }
    }

    public async Task<UrlDto> UpdateAsync(UrlDto urlDto)
    {
        try
        {
            var success = await _urlRepository.UpdateAsync(urlDto);
            if (!success)
            {
                throw new InvalidOperationException($"Failed to update URL with ID {urlDto.Id}");
            }
            return urlDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating URL with ID {UrlId}", urlDto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            return await _urlRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting URL with ID {UrlId}", id);
            throw;
        }
    }

    public async Task<string> GetOriginalUrlAndTrackClickAsync(string shortCode, string ipAddress, string userAgent, string referer)
    {
        try
        {
            var url = await _urlRepository.GetByShortCodeAsync(shortCode);
            if (url == null || !url.IsActive)
            {
                return string.Empty;
            }

            // Increment click count
            await _urlRepository.IncrementClickCountAsync(url.Id);

            // Extract device info from user agent
            string deviceType = GetDeviceType(userAgent);
            string browser = GetBrowser(userAgent);
            string operatingSystem = GetOperatingSystem(userAgent);

            // Record click data
            var clickData = new ClickDataDto
            {
                UrlId = url.Id,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ReferrerUrl = referer,
                DeviceType = deviceType,
                Browser = browser,
                OperatingSystem = operatingSystem,
                Country = "Unknown",
                City = "Unknown",
                ClickedAt = DateTime.UtcNow
            };

            await _clickDataRepository.CreateAsync(clickData);
            return url.OriginalUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking click for short code {ShortCode}", shortCode);
            return string.Empty;
        }
    }

    public async Task<bool> ShortCodeExistsAsync(string shortCode)
    {
        try
        {
            return await _urlRepository.ShortCodeExistsAsync(shortCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if short code {ShortCode} exists", shortCode);
            throw;
        }
    }

    public async Task<int> GetTotalUrlCountAsync()
    {
        try
        {
            return await _urlRepository.GetTotalUrlCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total URL count");
            throw;
        }
    }

    public async Task<int> GetActiveUrlCountAsync()
    {
        try
        {
            return await _urlRepository.GetActiveUrlCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active URL count");
            throw;
        }
    }

    public async Task<int> GetUrlCountByUserIdAsync(int userId)
    {
        try
        {
            return await _urlRepository.GetUrlCountByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting URL count for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteUrlAsync(string shortCode, int userId)
    {
        try
        {
            var url = await _urlRepository.GetByShortCodeAsync(shortCode);
            if (url == null)
            {
                return false;
            }

            // Check if URL belongs to user
            if (url.UserId != userId)
            {
                return false;
            }

            return await _urlRepository.DeleteAsync(url.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting URL with short code {ShortCode}", shortCode);
            throw;
        }
    }

    public async Task<UrlResponse> CreateShortUrlAsync(UrlCreationRequest request)
    {
        try
        {
            // Validate the original URL
            if (string.IsNullOrEmpty(request.OriginalUrl))
            {
                throw new InvalidOperationException("Original URL is required");
            }

            if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out _))
            {
                if (!request.OriginalUrl.StartsWith("http"))
                {
                    request.OriginalUrl = "http://" + request.OriginalUrl;
                    if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out _))
                    {
                        throw new InvalidOperationException("Invalid URL format");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid URL format");
                }
            }

            // Generate short code or use custom code if provided
            string shortCode;
            if (!string.IsNullOrEmpty(request.CustomCode))
            {
                // Custom code validation
                if (request.CustomCode.Length < 3 || request.CustomCode.Length > 20)
                {
                    throw new InvalidOperationException("Custom code must be between 3 and 20 characters");
                }

                if (!IsValidShortCode(request.CustomCode))
                {
                    throw new InvalidOperationException("Custom code contains invalid characters");
                }

                // Check if custom code already exists
                bool exists = await _urlRepository.ShortCodeExistsAsync(request.CustomCode);
                if (exists)
                {
                    throw new InvalidOperationException("Custom code already in use");
                }

                shortCode = request.CustomCode;
            }
            else
            {
                // Generate random short code
                shortCode = await GenerateUniqueShortCodeAsync();
            }

            // Set expiration date if specified
            DateTime? expiresAt = null;
            if (request.ExpiresInDays.HasValue && request.ExpiresInDays.Value > 0)
            {
                expiresAt = DateTime.UtcNow.AddDays(request.ExpiresInDays.Value);
            }

            // Create new URL entity
            var urlDto = new UrlDto
            {
                OriginalUrl = request.OriginalUrl,
                ShortCode = shortCode,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                ClickCount = 0,
                IsActive = true
            };

            // Save to database
            var createdUrl = await _urlRepository.CreateAsync(urlDto);

            // Build the short URL
            string baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            string shortUrl = $"{baseUrl}/{shortCode}";

            // Return the response
            return new UrlResponse
            {
                Id = createdUrl.Id,
                OriginalUrl = createdUrl.OriginalUrl,
                ShortUrl = shortUrl,
                ShortCode = shortCode,
                CreatedAt = createdUrl.CreatedAt,
                ExpiresAt = createdUrl.ExpiresAt,
                ClickCount = 0,
                IsActive = true
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating short URL for {OriginalUrl}", request.OriginalUrl);
            throw new InvalidOperationException("An error occurred while creating the short URL");
        }
    }

    public async Task<UrlResponse?> GetUrlByShortCodeAsync(string shortCode)
    {
        try
        {
            if (string.IsNullOrEmpty(shortCode))
            {
                return null;
            }

            var url = await _urlRepository.GetByShortCodeAsync(shortCode);
            if (url == null || !url.IsActive)
            {
                return null;
            }

            // Check if the URL has expired
            if (url.ExpiresAt.HasValue && url.ExpiresAt.Value < DateTime.UtcNow)
            {
                // Update URL status to inactive
                url.IsActive = false;
                await _urlRepository.UpdateAsync(url);
                return null;
            }

            // Build the short URL
            string baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            string shortUrl = $"{baseUrl}/{shortCode}";

            return new UrlResponse
            {
                Id = url.Id,
                OriginalUrl = url.OriginalUrl,
                ShortUrl = shortUrl,
                ShortCode = url.ShortCode,
                CreatedAt = url.CreatedAt,
                ExpiresAt = url.ExpiresAt,
                ClickCount = url.ClickCount,
                IsActive = url.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving URL for short code {ShortCode}", shortCode);
            return null;
        }
    }

    public async Task<bool> TrackClickAsync(string shortCode, ClickTrackingData trackingData)
    {
        try
        {
            var url = await _urlRepository.GetByShortCodeAsync(shortCode);
            if (url == null || !url.IsActive)
            {
                return false;
            }

            // Increment click count
            await _urlRepository.IncrementClickCountAsync(url.Id);

            // Record click data
            var clickData = new ClickDataDto
            {
                UrlId = url.Id,
                IpAddress = trackingData.IpAddress,
                UserAgent = trackingData.UserAgent,
                ReferrerUrl = trackingData.ReferrerUrl,
                DeviceType = trackingData.DeviceType,
                Browser = trackingData.Browser,
                OperatingSystem = trackingData.OperatingSystem,
                Country = trackingData.Country,
                City = trackingData.City,
                ClickedAt = DateTime.UtcNow
            };

            await _clickDataRepository.CreateAsync(clickData);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking click for short code {ShortCode}", shortCode);
            return false;
        }
    }

    public async Task<List<UrlResponse>> GetUrlsByUserIdAsync(int userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var urls = await _urlRepository.GetByUserIdAsync(userId, page, pageSize);
            string baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";

            return urls.Select(url => new UrlResponse
            {
                Id = url.Id,
                OriginalUrl = url.OriginalUrl,
                ShortUrl = $"{baseUrl}/{url.ShortCode}",
                ShortCode = url.ShortCode,
                CreatedAt = url.CreatedAt,
                ExpiresAt = url.ExpiresAt,
                ClickCount = url.ClickCount,
                IsActive = url.IsActive
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving URLs for user {UserId}", userId);
            return new List<UrlResponse>();
        }
    }

    private async Task<string> GenerateUniqueShortCodeAsync()
    {
        const int maxAttempts = 10;
        int attempts = 0;
        string shortCode;

        do
        {
            // Start with a shorter code length
            int codeLength = 6;
            
            // Increase length for subsequent attempts to reduce collision probability
            if (attempts > 3) codeLength = 7;
            if (attempts > 6) codeLength = 8;
            
            shortCode = GenerateRandomShortCode(codeLength);
            attempts++;
            
            // Check if the code already exists
            bool exists = await _urlRepository.ShortCodeExistsAsync(shortCode);
            if (!exists) break;
            
        } while (attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            throw new InvalidOperationException("Failed to generate a unique short code");
        }

        return shortCode;
    }

    private string GenerateRandomShortCode(int length)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
        var sb = new StringBuilder(length);
        
        for (int i = 0; i < length; i++)
        {
            sb.Append(validChars[_random.Next(validChars.Length)]);
        }
        
        return sb.ToString();
    }

    private bool IsValidShortCode(string code)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(code, "^[a-zA-Z0-9_-]+$");
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