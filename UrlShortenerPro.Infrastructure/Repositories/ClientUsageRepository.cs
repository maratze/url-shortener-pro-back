using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class ClientUsageRepository : IClientUsageRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ClientUsageRepository> _logger;

    public ClientUsageRepository(AppDbContext dbContext, ILogger<ClientUsageRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> CheckClientExistsAsync(string clientId)
    {
        try
        {
            return await _dbContext.ClientUsages.AnyAsync(u => u.ClientId == clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if client exists with ID {ClientId}", clientId);
            return false;
        }
    }

    public async Task<int> GetClientRequestCountAsync(string clientId)
    {
        try
        {
            var clientUsage = await _dbContext.ClientUsages
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ClientId == clientId);

            return clientUsage?.UsedRequests ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request count for client {ClientId}", clientId);
            return 0;
        }
    }

    public async Task<int> IncrementClientRequestCountAsync(string clientId)
    {
        try
        {
            var clientUsage = await _dbContext.ClientUsages
                .FirstOrDefaultAsync(u => u.ClientId == clientId);

            if (clientUsage == null)
            {
                // Create new client usage record
                clientUsage = new ClientUsage
                {
                    ClientId = clientId,
                    UsedRequests = 1,
                    LastRequestAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.ClientUsages.Add(clientUsage);
            }
            else
            {
                // Increment existing client usage
                clientUsage.UsedRequests++;
                clientUsage.LastRequestAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return clientUsage.UsedRequests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing request count for client {ClientId}", clientId);
            return 0;
        }
    }

    public async Task<DateTime?> GetFirstRequestDateAsync(string clientId)
    {
        try
        {
            var clientUsage = await _dbContext.ClientUsages
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ClientId == clientId);

            return clientUsage?.CreatedAt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first request date for client {ClientId}", clientId);
            return null;
        }
    }

    public async Task<bool> ResetClientRequestCountAsync(string clientId)
    {
        try
        {
            var clientUsage = await _dbContext.ClientUsages
                .FirstOrDefaultAsync(u => u.ClientId == clientId);

            if (clientUsage == null)
                return false;

            clientUsage.UsedRequests = 0;
            clientUsage.LastRequestAt = DateTime.UtcNow;
            clientUsage.CreatedAt = DateTime.UtcNow; // Reset creation date to current date

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting request count for client {ClientId}", clientId);
            return false;
        }
    }
} 