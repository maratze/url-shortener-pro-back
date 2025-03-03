using Microsoft.EntityFrameworkCore;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Interfaces;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Repositories;

public class ClientUsageRepository(AppDbContext context) : IClientUsageRepository
{
    public async Task<ClientUsage?> GetByClientIdAsync(string clientId)
    {
        return await context.ClientUsages
            .Where(x => x != null)
            .FirstOrDefaultAsync(u => u.ClientId == clientId);
    }

    public async Task AddAsync(ClientUsage clientUsage)
    {
        await context.ClientUsages.AddAsync(clientUsage);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClientUsage clientUsage)
    {
        context.ClientUsages.Update(clientUsage);
        await context.SaveChangesAsync();
    }
}