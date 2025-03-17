using System.ComponentModel.DataAnnotations;

namespace UrlShortenerPro.Infrastructure.Models;

public class User
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsPremium { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
        
    public ICollection<Url> Urls { get; set; } = new List<Url>();
}