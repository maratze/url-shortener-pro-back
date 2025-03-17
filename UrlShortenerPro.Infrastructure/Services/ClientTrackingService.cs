using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Infrastructure.Services;

public class ClientTrackingService : IClientTrackingService
{
    private readonly ILogger<ClientTrackingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _maxFreeRequests;
    private readonly IClientUsageRepository _clientUsageRepository;

    public ClientTrackingService(
        ILogger<ClientTrackingService> logger,
        IConfiguration configuration,
        IClientUsageRepository clientUsageRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _maxFreeRequests = int.Parse(_configuration["AppSettings:MaxFreeRequestsPerMonth"] ?? "10");
        _clientUsageRepository = clientUsageRepository;
    }

    public async Task<bool> IsClientAllowedAsync(string clientId, int maxRequestsPerMonth)
    {
        try
        {
            // Check if client exists
            var exists = await _clientUsageRepository.CheckClientExistsAsync(clientId);
            if (!exists)
            {
                // New client, allow and register
                await _clientUsageRepository.IncrementClientRequestCountAsync(clientId);
                return true;
            }

            // Get current request count
            var requestCount = await _clientUsageRepository.GetClientRequestCountAsync(clientId);
            var firstRequestDate = await _clientUsageRepository.GetFirstRequestDateAsync(clientId);

            // If first request was more than a month ago, reset counter
            if (firstRequestDate.HasValue && (DateTime.UtcNow - firstRequestDate.Value).TotalDays >= 30)
            {
                await _clientUsageRepository.ResetClientRequestCountAsync(clientId);
                await _clientUsageRepository.IncrementClientRequestCountAsync(clientId);
                return true;
            }

            // Check if client has exceeded limit
            if (requestCount >= maxRequestsPerMonth)
            {
                return false;
            }

            // Increment counter and allow
            await _clientUsageRepository.IncrementClientRequestCountAsync(clientId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking client usage for {ClientId}", clientId);
            return true; // Allow in case of error
        }
    }

    public async Task<int> GetRemainingFreeRequestsAsync(string clientId)
    {
        try
        {
            // Check if client exists
            var exists = await _clientUsageRepository.CheckClientExistsAsync(clientId);
            if (!exists)
            {
                return _maxFreeRequests;
            }

            // Get current request count
            var requestCount = await _clientUsageRepository.GetClientRequestCountAsync(clientId);
            var firstRequestDate = await _clientUsageRepository.GetFirstRequestDateAsync(clientId);

            // If first request was more than a month ago, reset counter
            if (firstRequestDate.HasValue && (DateTime.UtcNow - firstRequestDate.Value).TotalDays >= 30)
            {
                await _clientUsageRepository.ResetClientRequestCountAsync(clientId);
                return _maxFreeRequests;
            }

            // Calculate remaining requests
            int remainingRequests = Math.Max(0, _maxFreeRequests - requestCount);
            return remainingRequests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining requests for {ClientId}", clientId);
            return _maxFreeRequests; // Return max in case of error
        }
    }
} 