namespace UrlShortenerPro.Core.Interfaces;

public interface IClientTrackingService
{
    Task<int> GetRemainingFreeRequestsAsync(string clientId);
    Task DecrementFreeRequestsAsync(string clientId);
    Task<bool> HasFreeRequestsAvailableAsync(string clientId);
}