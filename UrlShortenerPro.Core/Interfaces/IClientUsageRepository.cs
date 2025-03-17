namespace UrlShortenerPro.Core.Interfaces;

public interface IClientUsageRepository
{
    Task<bool> CheckClientExistsAsync(string clientId);
    Task<int> GetClientRequestCountAsync(string clientId);
    Task<int> IncrementClientRequestCountAsync(string clientId);
    Task<DateTime?> GetFirstRequestDateAsync(string clientId);
    Task<bool> ResetClientRequestCountAsync(string clientId);
} 