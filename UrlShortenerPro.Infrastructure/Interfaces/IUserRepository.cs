using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}