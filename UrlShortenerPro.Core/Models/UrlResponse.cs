namespace UrlShortenerPro.Core.Models;

public class UrlResponse
{
    public int Id { get; set; }
    public string? OriginalUrl { get; set; }
    public string? ShortCode { get; set; }
    public string? ShortUrl { get; set; }
    public int? UserId { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public System.DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public int ClickCount { get; set; }
}