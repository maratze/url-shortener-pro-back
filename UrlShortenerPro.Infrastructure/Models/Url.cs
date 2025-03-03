namespace UrlShortenerPro.Infrastructure.Models;

public class Url
{
    public int Id { get; set; }
    public string? OriginalUrl { get; set; }
    public string? ShortCode { get; set; }
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public string? CustomCode { get; set; }
    public bool IsActive { get; set; } = true;
        
    public User? User { get; set; }
    public ICollection<ClickData> Clicks { get; set; } = new List<ClickData>();
}