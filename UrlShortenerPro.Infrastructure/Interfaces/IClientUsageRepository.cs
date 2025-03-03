using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Interfaces;

public interface IClientUsageRepository
{
    Task<ClientUsage?> GetByClientIdAsync(string clientId);
    Task AddAsync(ClientUsage clientUsage);
    Task UpdateAsync(ClientUsage clientUsage);
}