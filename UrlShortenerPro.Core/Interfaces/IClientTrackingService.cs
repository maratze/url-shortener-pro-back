namespace UrlShortenerPro.Core.Interfaces;

public interface IClientTrackingService
{
    Task<bool> IsClientAllowedAsync(string clientId, int maxRequestsPerMonth);
    Task<int> GetRemainingFreeRequestsAsync(string clientId);
}