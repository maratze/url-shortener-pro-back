using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Infrastructure.Interfaces;
using ClientUsage = UrlShortenerPro.Infrastructure.Models.ClientUsage;

namespace UrlShortenerPro.Core.Services;

public class ClientTrackingService(IClientUsageRepository repository) : IClientTrackingService
{
    private const int FREE_REQUESTS_LIMIT = 30;

    public async Task<int> GetRemainingFreeRequestsAsync(string clientId)
    {
        var usage = await repository.GetByClientIdAsync(clientId);
        if (usage == null)
        {
            return FREE_REQUESTS_LIMIT;
        }
            
        return Math.Max(0, FREE_REQUESTS_LIMIT - usage.UsedRequests);
    }
        
    public async Task DecrementFreeRequestsAsync(string clientId)
    {
        var usage = await repository.GetByClientIdAsync(clientId);
        if (usage == null)
        {
            usage = new ClientUsage
            {
                ClientId = clientId,
                UsedRequests = 1,
                LastRequestAt = DateTime.UtcNow
            };
            await repository.AddAsync(usage);
        }
        else
        {
            usage.UsedRequests++;
            usage.LastRequestAt = DateTime.UtcNow;
            await repository.UpdateAsync(usage);
        }
    }
        
    public async Task<bool> HasFreeRequestsAvailableAsync(string clientId)
    {
        return await GetRemainingFreeRequestsAsync(clientId) > 0;
    }
}